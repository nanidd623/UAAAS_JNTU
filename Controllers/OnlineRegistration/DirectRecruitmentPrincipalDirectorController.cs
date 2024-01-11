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

namespace UAAAS.Controllers.OnlineRegistration
{
    [ErrorHandling]
    public class DirectRecruitmentPrincipalDirectorController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,College")]
        public ActionResult Index()
        {
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var collgeextraactivities = db.jntuh_college_direct_principalrecruit.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var extraactivities = db.jntuh_direct_recruitment_principal.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            var extraactityIds = extraactivities.Select(i => i.id).ToArray();
            ViewBag.extraactivities = extraactivities;
            ViewBag.EnccollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            var experienceactivities = db.jntuh_college_direct_principal_experience.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Furnish Principal Details" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var compphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (compphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard"); ;
            }
            //int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
            //                                                                         editStatus.IsCollegeEditable == true &&
            //                                                                         editStatus.editFromDate <= todayDate &&
            //                                                                         editStatus.editToDate >= todayDate)
            //                                                    .Select(editStatus => editStatus.id)
            //                                                    .FirstOrDefault();
            //if (status == 0 && Roles.IsUserInRole("College"))
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("View", "CollegeExtraCurricularActivities");
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //    var isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("VAP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
            //    if (isPageEditable)
            //    {
            //        ViewBag.IsEditable = true;
            //    }
            //    else
            //    {
            //        ViewBag.IsEditable = false;
            //        return RedirectToAction("View", "CollegeExtraCurricularActivities");
            //    }
            //}
            var clgextraactivities = collgeextraactivities.Where(i => extraactityIds.Contains(i.activityid)).ToList();
            if (clgextraactivities.Count > 0)
            {
                return RedirectToAction("View");
            }

            var expact = experienceactivities.Where(i => i.activityid == 5).ToList();
            var principalExperiences = new List<CollegeDirectPrinicpalExperience>();
            foreach (var act in expact)
            {
                var Experiences = new CollegeDirectPrinicpalExperience()
                {
                    activityid = act.activityid,
                    collegeid = Convert.ToInt32(act.collegeid),
                    designationid = Convert.ToInt32(act.facultydesignationid),
                    registrationnumber = act.registrationnumber,
                    fromdate = act.fromdate != null ? act.fromdate.ToString() : "",
                    todate = act.todate != null ? act.todate.ToString() : "",
                    supportingdoc1path = act.supportingdocs1,
                    supportingdoc2path = act.supportingdocs2,
                    supportingdoc3path = act.supportingdocs3,
                    //isactive =  act.isactive,
                    createdon = act.createdon,
                    createdby = act.createdby
                };
                principalExperiences.Add(Experiences);
            }
            ViewBag.principalExperiences = principalExperiences;
            var lstactivities = new List<CollegeDirectPrinicpal>();
            foreach (var item in extraactivities)
            {
                var objactivities = new CollegeDirectPrinicpal()
                {
                    activityid = item.id,
                    //activitystatus = false,
                    activitydoc = null,
                    activitydocpath = string.Empty
                };
                if (principalExperiences.Count > 0 && item.id == 5)
                {
                    objactivities.activitystatus = true;
                }
                lstactivities.Add(objactivities);
            }
            return View(lstactivities);
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpPost]
        public ActionResult Create(List<CollegeDirectPrinicpal> extracurricularInformation)
        {
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var schoalarsActivityStatus = false;
            var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Furnish Principal Details" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var compphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (compphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard"); ;
            }
            if (userCollegeID > 0)
            {
                foreach (var item in extracurricularInformation)
                {
                    var objextraactivities = new jntuh_college_direct_principalrecruit()
                    {
                        activityid = item.activityid,
                        academicyear = ay0,
                        activitystatus = item.activitystatus,
                        collegeid = userCollegeID,
                        createdby = userID,
                        createdon = DateTime.Now,
                        isactive = true,
                        registrationnumber = string.Empty
                    };
                    if (item.activityid == 1 && item.activitystatus == true)
                    {
                        schoalarsActivityStatus = true;
                    }
                    if ((objextraactivities.activityid == 2 || objextraactivities.activityid == 3) && schoalarsActivityStatus == true)
                    {
                        //objextraactivities.remarks = item.remarks;
                        objextraactivities.activitystatus = true;
                        objextraactivities.remarks = item.noofphdscholars;
                    }
                    if (item.activityid == 4 && item.activitystatus == true)
                    {
                        objextraactivities.remarks = item.noofphdscholars;
                    }
                    if (item.activitydoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/College/DirectPrinicpalRecruit";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(item.activitydoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (item.activitydocpath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                item.activitydoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                item.activitydocpath = string.Format("{0}{1}", fileName, ext);
                                objextraactivities.supportingdocuments = item.activitydocpath;
                            }
                            else
                            {
                                item.activitydoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), item.activitydocpath));
                                objextraactivities.supportingdocuments = item.activitydocpath;
                            }
                        }
                    }
                    db.jntuh_college_direct_principalrecruit.Add(objextraactivities);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("View");
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpGet]
        public ActionResult FacultyExperienceAdd(int activitytypeId, string enccollegeId)
        {
            var todayDate = DateTime.Now;
            var collegeId = 0;
            //var desigIds = new int[] { 1, 2, 3 };
            List<SelectListItem> designations = new List<SelectListItem>(){
                new SelectListItem() { Text = "Professor", Value = "1" },
                new SelectListItem() { Text = "Associate Professor", Value = "2" },
                new SelectListItem() { Text = "Assistant Professor", Value = "3" },
                new SelectListItem() { Text = "Research", Value = "4" },
                new SelectListItem() { Text = "Industry", Value = "5" },
                new SelectListItem() { Text = "Principal", Value = "6" },
                new SelectListItem() { Text = "Lecturer", Value = "7" }
            };
            //ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true && desigIds.Contains(c.id)).ToList();
            ViewBag.designation = designations.ToList();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Furnish Principal Details" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var compphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (compphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard"); ;
            }
            if (!string.IsNullOrEmpty(enccollegeId) && activitytypeId == 5)
            {
                collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(enccollegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

                //var directorPrincipalexp = new CollegeDirectPrinicpalExperience() { collegeid = collegeId, activityid = activitytypeId };
                var collgeextraactivities = db.jntuh_college_direct_principalrecruit.Where(i => i.isactive == true && i.collegeid == collegeId).ToList();
                var extraactivities = db.jntuh_direct_recruitment_principal.Where(i => i.isactive == true && i.activitytype == 1).ToList();
                var extraactityIds = extraactivities.Select(i => i.id).ToArray();
                var clgextraactivities = collgeextraactivities.Where(i => extraactityIds.Contains(i.activityid)).ToList();
                var regfaculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
                //if (clgextraactivities.Count > 0)
                //{
                //    return RedirectToAction("View");
                //}
                var CollegeDirectPrinicpalExperience = new List<CollegeDirectPrinicpalExperience>();
                //var designations = db.jntuh_designation.Where(c => c.isActive == true && desigIds.Contains(c.id)).ToList();
                var directorPrincipalexps = db.jntuh_college_direct_principal_experience.Where(i => i.isactive == true && i.collegeid == collegeId && i.activityid == activitytypeId).ToList();
                foreach (var act in directorPrincipalexps)
                {
                    var facultydetails = regfaculty.Where(i => i.RegistrationNumber == act.registrationnumber).FirstOrDefault();
                    var exp = new CollegeDirectPrinicpalExperience()
                    {
                        activityid = act.activityid,
                        encactivityId = UAAAS.Models.Utilities.EncryptString(act.id.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                        enccollegeId = enccollegeId,
                        collegeid = Convert.ToInt32(act.collegeid),
                        designation = act.facultydesignationid != null ? designations.Where(i => i.Value == act.facultydesignationid.ToString()).FirstOrDefault().Text : "",
                        registrationnumber = act.registrationnumber,
                        presentlyWorking = act.presentlyworking.ToString(),
                        workingorganisationName = act.workinginstitutionorganisationname,
                        facultyName = facultydetails != null ? facultydetails.FirstName + ' ' + facultydetails.LastName : "",
                        fromdate = act.fromdate != null ? Convert.ToDateTime(act.fromdate).ToString("dd-MM-yyyy") : "",
                        todate = act.todate != null ? Convert.ToDateTime(act.todate).ToString("dd-MM-yyyy") : "",
                        supportingdoc1path = act.supportingdocs1,
                        supportingdoc2path = act.supportingdocs2,
                        supportingdoc3path = act.supportingdocs3,
                        displayExperience = DisplayTotalExperience(act.fromdate.ToString().Split(' ')[0], act.todate.ToString().Split(' ')[0]),
                        //isactive =  act.isactive,
                        createdon = act.createdon,
                        createdby = act.createdby
                    };
                    CollegeDirectPrinicpalExperience.Add(exp);
                }
                ViewBag.CollegeDirectPrinicpalExperience = CollegeDirectPrinicpalExperience;
                var directorPrincipalexp = CollegeDirectPrinicpalExperience.Count > 0 ? new CollegeDirectPrinicpalExperience() { collegeid = collegeId, activityid = activitytypeId, registrationnumber = CollegeDirectPrinicpalExperience.LastOrDefault().registrationnumber } : new CollegeDirectPrinicpalExperience() { collegeid = collegeId, activityid = activitytypeId };
                return View(directorPrincipalexp);
            }
            else
            {
                return RedirectToAction("Index", "DirectRecruitmentPrincipalDirector");
            }
            return RedirectToAction("Index", "DirectRecruitmentPrincipalDirector");
        }

        public static string DisplayTotalExperience(string facultyDateOfAppointment, string facultyDateOfResignation)
        {
            int TotalExperienceyears = 0;
            int TotalExperienceMonths = 0;
            int TotalExperiencedays = 0;
            var TotalFullExperience = "";
            if (!string.IsNullOrEmpty(facultyDateOfAppointment) && !string.IsNullOrEmpty(facultyDateOfResignation))
            {
                int fromyear = Convert.ToInt32(facultyDateOfAppointment.Split('/')[2]);
                int frommonth = Convert.ToInt32(facultyDateOfAppointment.Split('/')[0]);
                int fromdate = Convert.ToInt32(facultyDateOfAppointment.Split('/')[1]);

                int year = Convert.ToInt32(facultyDateOfResignation.Split('/')[2]);
                int month = Convert.ToInt32(facultyDateOfResignation.Split('/')[0]);
                int date = Convert.ToInt32(facultyDateOfResignation.Split('/')[1]);
                DateTime zeroTime = new DateTime(1, 1, 1);

                DateTime olddate = new DateTime(fromyear, frommonth, fromdate);
                DateTime curdate = new DateTime(year, month, date);

                var Difference = curdate - olddate;
                if (Difference.Days > 0)
                {
                    int years = (zeroTime + Difference).Year - 1;
                    int months = (zeroTime + Difference).Month - 1;
                    int days = (zeroTime + Difference).Day;

                    var TotalExperience = years + " Years " + months + " Months " + days + "Days";
                    TotalExperienceyears = TotalExperienceyears + years;
                    TotalExperienceMonths = TotalExperienceMonths + months;
                    TotalExperiencedays = TotalExperiencedays + days;
                    TotalFullExperience = TotalExperienceyears + " Years " + TotalExperienceMonths + " Months " + TotalExperiencedays + "Days";
                }
            }

            return TotalFullExperience;
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult FacultyExperienceAdd(CollegeDirectPrinicpalExperience faculty)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //var isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var enccollegeid = UAAAS.Models.Utilities.EncryptString(faculty.collegeid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            List<SelectListItem> designations = new List<SelectListItem>(){
                new SelectListItem() { Text = "Professor", Value = "1" },
                new SelectListItem() { Text = "Associate Professor", Value = "2" },
                new SelectListItem() { Text = "Assistant Professor", Value = "3" },
                new SelectListItem() { Text = "Research", Value = "4" },
                new SelectListItem() { Text = "Industry", Value = "5" },
                new SelectListItem() { Text = "Principal", Value = "6" },
                new SelectListItem() { Text = "Lecturer", Value = "7" }
            };
            if (faculty != null)
            {
                var collegeCode = db.jntuh_college.Where(c => c.id == faculty.collegeid).Select(c => c.collegeCode).FirstOrDefault();
                if (faculty.supportingdoc1 != null)
                {
                    var affiliationfile = "~/Content/Upload/College/DirectPrinicpalRecruit/PrincipalExperience";
                    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    }
                    var ext = Path.GetExtension(faculty.supportingdoc1.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = collegeCode + "-AJR" + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                        faculty.supportingdoc1.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                        faculty.supportingdoc1path = string.Format("{0}{1}", fileName, ext);
                    }
                }

                if (faculty.supportingdoc2 != null)
                {
                    var affiliationfile = "~/Content/Upload/College/DirectPrinicpalRecruit/PrincipalExperience";
                    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    }
                    var ext = Path.GetExtension(faculty.supportingdoc2.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = collegeCode + "-RL" + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                        faculty.supportingdoc2.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                        faculty.supportingdoc2path = string.Format("{0}{1}", fileName, ext);
                    }
                }


                if (faculty.supportingdoc3 != null)
                {
                    var affiliationfile = "~/Content/Upload/College/DirectPrinicpalRecruit/PrincipalExperience";
                    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    }
                    var ext = Path.GetExtension(faculty.supportingdoc3.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = collegeCode + "-SCM" + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                        faculty.supportingdoc3.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                        faculty.supportingdoc3path = string.Format("{0}{1}", fileName, ext);
                    }
                }

                var direct_principal_experience = new jntuh_college_direct_principal_experience()
                {
                    activityid = faculty.activityid,
                    collegeid = faculty.collegeid,
                    facultydesignationid = faculty.designationid,
                    presentlyworking = faculty.presentlyWorking == "True" ? true : false,
                    workinginstitutionorganisationname = faculty.workingorganisationName,
                    facultydesignation = designations.Where(i => i.Value == faculty.designationid.ToString()) != null ? designations.Where(i => i.Value == faculty.designationid.ToString()).FirstOrDefault().Text : "",
                    registrationnumber = faculty.registrationnumber,
                    fromdate = Utilities.DDMMYY2MMDDYY(faculty.fromdate),
                    todate = faculty.todate != null ? Utilities.DDMMYY2MMDDYY(faculty.todate) : Utilities.DDMMYY2MMDDYY("01/01/1900"),
                    supportingdocs1 = faculty.supportingdoc1path,
                    supportingdocs2 = faculty.supportingdoc2path,
                    supportingdocs3 = faculty.supportingdoc3path,
                    isactive = true,
                    createdby = userID,
                    createdon = DateTime.Now
                };
                db.jntuh_college_direct_principal_experience.Add(direct_principal_experience);
                db.SaveChanges();
                TempData["Success"] = "Experience Added Successfully.";
                return RedirectToAction("FacultyExperienceAdd", "DirectRecruitmentPrincipalDirector", new { activitytypeId = faculty.activityid, enccollegeId = enccollegeid });
            }
            else
            {
                return RedirectToAction("Index", "DirectRecruitmentPrincipalDirector");
            }
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        public ActionResult Edit(string collegeId)
        {
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Furnish Principal Details" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var compphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (compphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard"); ;
            }
            var collgeextraactivities = db.jntuh_college_direct_principalrecruit.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var extraactivities = db.jntuh_direct_recruitment_principal.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            var extraactityIds = extraactivities.Select(i => i.id).ToArray();
            ViewBag.extraactivities = extraactivities;
            ViewBag.EnccollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            var experienceactivities = db.jntuh_college_direct_principal_experience.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            //int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
            //                                                                         editStatus.IsCollegeEditable == true &&
            //                                                                         editStatus.editFromDate <= todayDate &&
            //                                                                         editStatus.editToDate >= todayDate)
            //                                                    .Select(editStatus => editStatus.id)
            //                                                    .FirstOrDefault();
            //if (status == 0 && Roles.IsUserInRole("College"))
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("View", "CollegeExtraCurricularActivities");
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //    var isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("VAP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
            //    if (isPageEditable)
            //    {
            //        ViewBag.IsEditable = true;
            //    }
            //    else
            //    {
            //        ViewBag.IsEditable = false;
            //        return RedirectToAction("View", "CollegeExtraCurricularActivities");
            //    }
            //}
            //var clgextraactivities = collgeextraactivities.Where(i => extraactityIds.Contains(i.activityid)).ToList();
            //if (clgextraactivities.Count > 0)
            //{
            //    return RedirectToAction("View");
            //}

            var expact = experienceactivities.Where(i => i.activityid == 5).ToList();
            var principalExperiences = new List<CollegeDirectPrinicpalExperience>();
            foreach (var act in expact)
            {
                var Experiences = new CollegeDirectPrinicpalExperience()
                {
                    activityid = act.activityid,
                    collegeid = Convert.ToInt32(act.collegeid),
                    designationid = Convert.ToInt32(act.facultydesignationid),
                    registrationnumber = act.registrationnumber,
                    fromdate = act.fromdate != null ? act.fromdate.ToString() : "",
                    todate = act.todate != null ? act.todate.ToString() : "",
                    supportingdoc1path = act.supportingdocs1,
                    supportingdoc2path = act.supportingdocs2,
                    supportingdoc3path = act.supportingdocs3,
                    //isactive =  act.isactive,
                    createdon = act.createdon,
                    createdby = act.createdby
                };
                principalExperiences.Add(Experiences);
            }
            ViewBag.principalExperiences = principalExperiences;
            var lstactivities = new List<CollegeDirectPrinicpal>();
            foreach (var item in collgeextraactivities)
            {
                var jntuhExtracurricularactivities = extraactivities.Where(i => i.id == item.activityid).FirstOrDefault();
                var objactivities = new CollegeDirectPrinicpal()
                {
                    activityid = item.id,
                    activityDesc = jntuhExtracurricularactivities.activitydescription,
                    activitystatus = item.activitystatus,
                    supportingdocuments = item.supportingdocuments,
                    activitydocpath = item.supportingdocuments,
                    activitydoc = null,
                    remarks = item.remarks,
                    noofphdscholars = (item.activityid == 2 || item.activityid == 3 || item.activityid == 4) ? item.remarks : "0"
                };
                if (principalExperiences.Count > 0 && item.id == 5)
                {
                    objactivities.activitystatus = true;
                }
                lstactivities.Add(objactivities);
            }
            if (lstactivities.Count == 0)
            {
                return RedirectToAction("Index");
            }
            return View(lstactivities);
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpPost]
        public ActionResult Edit(List<CollegeDirectPrinicpal> extracurricularInformation)
        {
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var schoalarsActivityStatus = false;
            var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Furnish Principal Details" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var compphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (compphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard"); ;
            }
            var collegeExtraActivities = db.jntuh_college_direct_principalrecruit.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            if (userCollegeID > 0)
            {
                foreach (var item in collegeExtraActivities)
                {
                    var collegeAct = extracurricularInformation.FirstOrDefault(i => i.activityid == item.activityid);
                    if (collegeAct != null)
                    {
                        item.activitystatus = collegeAct.activitystatus;
                        item.updatedby = userID;
                        item.updatedon = DateTime.Now;
                        if (item.activityid == 1 && item.activitystatus == true)
                        {
                            schoalarsActivityStatus = true;
                        }
                        if ((item.activityid == 2 || item.activityid == 3) && schoalarsActivityStatus == true)
                        {
                            //objextraactivities.remarks = item.remarks;
                            item.activitystatus = true;
                            item.remarks = collegeAct.noofphdscholars;
                        }
                        if (item.activityid == 4 && item.activitystatus == true)
                        {
                            item.remarks = collegeAct.noofphdscholars;
                        }

                        if (collegeAct.activitydoc != null)
                        {
                            if (collegeAct.activitydoc.FileName != item.supportingdocuments)
                            {
                                var affiliationfile = "~/Content/Upload/College/DirectPrinicpalRecruit";
                                if (!Directory.Exists(Server.MapPath(affiliationfile)))
                                {
                                    Directory.CreateDirectory(Server.MapPath(affiliationfile));
                                }
                                var ext = Path.GetExtension(collegeAct.activitydoc.FileName);
                                if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                                {
                                    string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                    collegeAct.activitydoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                    collegeAct.activitydocpath = string.Format("{0}{1}", fileName, ext);
                                    item.supportingdocuments = collegeAct.activitydocpath;
                                }
                            }
                        }
                    }
                    if (item.activitystatus == false)
                    {
                        item.supportingdocuments = null;
                    }
                    //var objextraactivities = new jntuh_college_direct_principalrecruit()
                    //{
                    //    activityid = item.activityid,
                    //    academicyear = ay0,
                    //    activitystatus = item.activitystatus,
                    //    collegeid = userCollegeID,
                    //    createdby = userID,
                    //    createdon = DateTime.Now,
                    //    isactive = true,
                    //    registrationnumber = string.Empty
                    //};
                    //if (item.activityid == 1 && item.activitystatus == true)
                    //{
                    //    schoalarsActivityStatus = true;
                    //}
                    //if ((objextraactivities.activityid == 2 || objextraactivities.activityid == 3) && schoalarsActivityStatus == true)
                    //{
                    //    //objextraactivities.remarks = item.remarks;
                    //    objextraactivities.activitystatus = true;
                    //    objextraactivities.remarks = item.noofphdscholars;
                    //}
                    //if (item.activityid == 4 && item.activitystatus == true)
                    //{
                    //    objextraactivities.remarks = item.noofphdscholars;
                    //}
                    //if (item.activitydoc != null)
                    //{
                    //    var affiliationfile = "~/Content/Upload/College/DirectPrinicpalRecruit";
                    //    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    //    {
                    //        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    //    }
                    //    var ext = Path.GetExtension(item.activitydoc.FileName);
                    //    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    //    {
                    //        if (item.activitydocpath == null)
                    //        {
                    //            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                    //            item.activitydoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                    //            item.activitydocpath = string.Format("{0}{1}", fileName, ext);
                    //            objextraactivities.supportingdocuments = item.activitydocpath;
                    //        }
                    //        else
                    //        {
                    //            item.activitydoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), item.activitydocpath));
                    //            objextraactivities.supportingdocuments = item.activitydocpath;
                    //        }
                    //    }
                    //}
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("View");
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        public ActionResult View()
        {
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var masteractivities = db.jntuh_direct_recruitment_principal.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            var extraactivities = db.jntuh_college_direct_principalrecruit.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var experienceactivities = db.jntuh_college_direct_principal_experience.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Furnish Principal Details" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var compphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (compphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard"); ;
            }
            else
            {
                ViewBag.IsEditable = true;
            }
            ViewBag.EnccollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            var lstactivities = (from item in extraactivities
                                 let jntuhExtracurricularactivities = masteractivities.FirstOrDefault(i => i.id == item.activityid)
                                 where jntuhExtracurricularactivities != null
                                 select new CollegeDirectPrinicpal()
                                 {
                                     activityid = item.activityid,
                                     activityDesc = jntuhExtracurricularactivities.activitydescription,
                                     activitystatus = item.activitystatus,
                                     supportingdocuments = item.supportingdocuments,
                                     remarks = item.remarks
                                 }).ToList();
            foreach (var item in lstactivities)
            {
                if (item.activityid == 5)
                {
                    var expact = experienceactivities.Where(i => i.activityid == item.activityid).ToList();
                    var principalExperiences = new List<CollegeDirectPrinicpalExperience>();
                    foreach (var act in expact)
                    {
                        var Experiences = new CollegeDirectPrinicpalExperience()
                        {
                            activityid = act.activityid,
                            collegeid = Convert.ToInt32(act.collegeid),
                            designationid = Convert.ToInt32(act.facultydesignationid),
                            registrationnumber = act.registrationnumber,
                            fromdate = act.fromdate != null ? act.fromdate.ToString() : "",
                            todate = act.todate != null ? act.todate.ToString() : "",
                            supportingdoc1path = act.supportingdocs1,
                            supportingdoc2path = act.supportingdocs2,
                            supportingdoc3path = act.supportingdocs3,
                            //isactive =  act.isactive,
                            createdon = act.createdon,
                            createdby = act.createdby
                        };
                        principalExperiences.Add(Experiences);
                    }
                    item.principalExperiences = principalExperiences;
                }
            }
            //int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
            //                                                                         editStatus.IsCollegeEditable == true &&
            //                                                                         editStatus.editFromDate <= todayDate &&
            //                                                                         editStatus.editToDate >= todayDate)
            //                                                    .Select(editStatus => editStatus.id)
            //                                                    .FirstOrDefault();
            //if (status == 0 && Roles.IsUserInRole("College"))
            //{
            //    ViewBag.IsEditable = false;
            //    return View(lstactivities);
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //    bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("VAP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
            //    if (isPageEditable)
            //    {
            //        ViewBag.IsEditable = true;
            //    }
            //    else
            //    {
            //        ViewBag.IsEditable = false;
            //        return View(lstactivities);
            //    }
            //}
            if (lstactivities.Count == 0)
            {
                return RedirectToAction("Index");
            }
            return View(lstactivities);
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpGet]
        public ActionResult FacultyExperiencesView(int activitytypeId, string enccollegeId)
        {
            var collegeId = 0;
            if (!string.IsNullOrEmpty(enccollegeId))
            {
                collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(enccollegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            //var directorPrincipalexp = new CollegeDirectPrinicpalExperience() { collegeid = collegeId, activityid = activitytypeId };
            var CollegeDirectPrinicpalExperience = new List<CollegeDirectPrinicpalExperience>();
            //var desigIds = new int[] { 1, 2, 3 };
            //var designations = db.jntuh_designation.Where(c => c.isActive == true && desigIds.Contains(c.id)).ToList();
            var regfaculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            List<SelectListItem> designations = new List<SelectListItem>(){
                new SelectListItem() { Text = "Professor", Value = "1" },
                new SelectListItem() { Text = "Associate Professor", Value = "2" },
                new SelectListItem() { Text = "Assistant Professor", Value = "3" },
                new SelectListItem() { Text = "Research", Value = "4" },
                new SelectListItem() { Text = "Industry", Value = "5" },
                new SelectListItem() { Text = "Principal", Value = "6" },
                new SelectListItem() { Text = "Lecturer", Value = "7" }
            };
            var directorPrincipalexp = db.jntuh_college_direct_principal_experience.Where(i => i.isactive == true && i.collegeid == collegeId && i.activityid == activitytypeId).ToList();
            foreach (var act in directorPrincipalexp)
            {
                var facultydetails = regfaculty.Where(i => i.RegistrationNumber == act.registrationnumber).FirstOrDefault();
                var exp = new CollegeDirectPrinicpalExperience()
                {
                    activityid = act.activityid,
                    collegeid = Convert.ToInt32(act.collegeid),
                    designation = act.facultydesignationid != null ? designations.Where(i => i.Value == act.facultydesignationid.ToString()).FirstOrDefault().Text : "",
                    registrationnumber = act.registrationnumber,
                    facultyName = facultydetails != null ? facultydetails.FirstName + ' ' + facultydetails.LastName : "",
                    fromdate = act.fromdate != null ? Convert.ToDateTime(act.fromdate).ToString("dd-MM-yyyy") : "",
                    todate = act.todate != null ? Convert.ToDateTime(act.todate).ToString("dd-MM-yyyy") : "",
                    supportingdoc1path = act.supportingdocs1,
                    supportingdoc2path = act.supportingdocs2,
                    supportingdoc3path = act.supportingdocs3,
                    //isactive =  act.isactive,
                    createdon = act.createdon,
                    createdby = act.createdby
                };
                CollegeDirectPrinicpalExperience.Add(exp);
            }
            ViewBag.CollegeDirectPrinicpalExperience = CollegeDirectPrinicpalExperience;
            return PartialView(new List<CollegeDirectPrinicpalExperience>());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public JsonResult CheckPrincipalRegistrationNumber(string registrationnumber)
        {
            var RegistrationNumber = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber == registrationnumber.Trim()).Select(s => s).FirstOrDefault();

            if (RegistrationNumber != null)
            {
                var isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == registrationnumber.Trim()).Select(r => r.RegistrationNumber).FirstOrDefault();
                if (RegistrationNumber.Blacklistfaculy == true)
                {
                    return Json("Registration Number is in Blacklist.", JsonRequestBehavior.AllowGet);
                }
                else if (RegistrationNumber.AbsentforVerification == true)
                {
                    return Json("Registration Number is in Inactive.", JsonRequestBehavior.AllowGet);
                }
                //else if (isExistingFaculty != null)
                //{
                //    return Json("Faculty is already working.", JsonRequestBehavior.AllowGet);
                //}
                else
                    return Json(true);
            }
            else
                return Json("Invalid Registration.", JsonRequestBehavior.AllowGet);

        }


        [Authorize(Roles = "College,Admin,SuperAdmin")]
        public ActionResult DeleteExperience(string encactivityId, string enccollegeid)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var activityId = 0;
            if (!string.IsNullOrEmpty(encactivityId))
            {
                activityId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(encactivityId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            try
            {
                if (activityId > 0)
                {
                    var data = db.jntuh_college_direct_principal_experience.Find(activityId);
                    db.jntuh_college_direct_principal_experience.Remove(data);
                    db.SaveChanges();
                    if (true)
                    {
                        TempData["Success"] = "Experience Deleted successfully.";
                    }
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error while deleting..";
            }

            return RedirectToAction("FacultyExperienceAdd", "DirectRecruitmentPrincipalDirector", new { activitytypeId = 5, enccollegeId = enccollegeid });
        }
    }
}
