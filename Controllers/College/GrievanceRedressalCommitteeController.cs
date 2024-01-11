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

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class GrievanceRedressalCommitteeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.PreviousYear = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(s => s.academicYear).FirstOrDefault();

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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            IEnumerable<GrievanceRedressalCommittee> committee = (from gc in db.jntuh_college_grievance_committee
                                                                  join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                                  join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                                  where gc.collegeId == userCollegeID
                                                                  select new GrievanceRedressalCommittee
                                                                   {
                                                                       id = gc.id,
                                                                       //rowNumber = index + 1,
                                                                       collegeId = gc.collegeId,
                                                                       memberDesignation = gc.memberDesignation,
                                                                       memberName = gc.memberName,
                                                                       designationName = d.Designation,
                                                                       my_aspnet_users = gc.my_aspnet_users,
                                                                       my_aspnet_users1 = gc.my_aspnet_users1,
                                                                       actualDesignationId = gc.actualDesignation,
                                                                       actualDesignation = ad.designation,
                                                                       Email = gc.Email,
                                                                       Mobile = gc.Mobile
                                                                   }).OrderBy(r => r.memberName).ToList();

            List<GrievanceRedressalComplaints> complaints = db.jntuh_college_grievance_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new GrievanceRedressalComplaints
                                              {
                                                  id = a.id,
                                                  //rowNumber = index + 1,
                                                  collegeId = a.collegeId,
                                                  complaintReceived = a.complaintReceived,
                                                  actionsTaken = a.actionsTaken,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                              }).OrderBy(r => r.actionsTaken).ToList();

            ViewBag.Committee = committee;
            ViewBag.Complaints = complaints;
            ViewBag.CommitteeCount = committee.Count();
            ViewBag.ComplaintsCount = complaints.Count();
            DateTime todayDate = DateTime.Now.Date;

            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            //Getting Support Document
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Grievance Redressal Proceedings")
                    .Select(e => e.id)
                    .FirstOrDefault();
            ViewBag.SupportDocument =
                db.jntuh_college_enclosures.Where(
                    r => r.collegeID == userCollegeID && r.academicyearId == prAy && r.enclosureId == enclosureId)
                    .Select(s => s.path)
                    .FirstOrDefault();
            ViewBag.collegeGRCpath = string.Empty;
            var grcDate = Convert.ToDateTime("2021-06-07 13:44:51");
            var collegegrcNews = db.jntuh_college_news.Where(i => i.collegeId == userCollegeID && i.createdOn == grcDate).ToList();
            if (collegegrcNews.Count > 0)
            {
                ViewBag.collegeGRCpath = collegegrcNews.FirstOrDefault().navigateURL;
            }
            if (committee.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.CommitteeNotUpload = true;
            }
            else
            {
                ViewBag.CommitteeNotUpload = false;
            }
            if (complaints.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.ComplaintsNotUpload = true;
            }
            else
            {
                ViewBag.ComplaintsNotUpload = false;
            }
            if (status == 0 && (committee.Count() > 0 || complaints.Count() > 0) && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "GrievanceRedressalCommittee");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("GR") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "GrievanceRedressalCommittee");
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            ViewBag.actualDesignation = db.jntuh_designation.Where(ad => ad.isActive == true).Select(ad => ad).ToList();
            ViewBag.Designation = db.jntuh_grc_designation.Where(u => u.isActive == true).Select(u => u).ToList();
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    GrievanceRedressalCommittee committee = db.jntuh_college_grievance_committee.Where(oc => oc.id == id).Select(a =>
                                              new GrievanceRedressalCommittee
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  memberDesignation = a.memberDesignation,
                                                  memberName = a.memberName,
                                                  Email = a.Email,
                                                  Mobile = a.Mobile,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  updatedBy = a.updatedBy,
                                                  updatedOn = a.updatedOn,
                                                  jntuh_college = a.jntuh_college,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                                  actualDesignationId = a.actualDesignation
                                              }).FirstOrDefault();
                    return PartialView("_GrievanceRedressalCommitteeData", committee);
                }
                else
                {
                    GrievanceRedressalCommittee committee = new GrievanceRedressalCommittee();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        committee.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_GrievanceRedressalCommitteeData", committee);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    GrievanceRedressalCommittee uCommittee = db.jntuh_college_grievance_committee.Where(oc => oc.id == id).Select(a =>
                                               new GrievanceRedressalCommittee
                                               {
                                                   id = a.id,
                                                   collegeId = a.collegeId,
                                                   memberDesignation = a.memberDesignation,
                                                   memberName = a.memberName,
                                                   Email = a.Email,
                                                   Mobile = a.Mobile,
                                                   createdBy = a.createdBy,
                                                   createdOn = a.createdOn,
                                                   updatedBy = a.updatedBy,
                                                   updatedOn = a.updatedOn,
                                                   jntuh_college = a.jntuh_college,
                                                   my_aspnet_users = a.my_aspnet_users,
                                                   my_aspnet_users1 = a.my_aspnet_users1,
                                                   actualDesignationId = a.actualDesignation
                                               }).FirstOrDefault();
                    return View("GrievanceRedressalCommitteeData", uCommittee);
                }
                else
                {
                    GrievanceRedressalCommittee ucommittee = new GrievanceRedressalCommittee();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        ucommittee.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("GrievanceRedressalCommitteeData", ucommittee);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(GrievanceRedressalCommittee Committee, string cmd)
        {
            ViewBag.actualDesignation = db.jntuh_designation.Where(ad => ad.isActive == true).Select(ad => ad).ToList();
            ViewBag.Designation = db.jntuh_grc_designation.Where(u => u.isActive == true).Select(u => u).ToList();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Committee.collegeId;
            }
            if (ModelState.IsValid)
            {

                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_college_grievance_committee jntuh_college_grievance_committee = new jntuh_college_grievance_committee();
                        jntuh_college_grievance_committee.id = Committee.id;
                        jntuh_college_grievance_committee.collegeId = userCollegeID;
                        jntuh_college_grievance_committee.memberDesignation = Committee.memberDesignation;
                        jntuh_college_grievance_committee.memberName = Committee.memberName;
                        jntuh_college_grievance_committee.Email = Committee.Email;
                        jntuh_college_grievance_committee.Mobile = Committee.Mobile;
                        jntuh_college_grievance_committee.createdBy = userID;
                        jntuh_college_grievance_committee.createdOn = DateTime.Now;
                        jntuh_college_grievance_committee.actualDesignation = Committee.actualDesignationId;

                        db.jntuh_college_grievance_committee.Add(jntuh_college_grievance_committee);
                        db.SaveChanges();
                        TempData["Success"] = "Grievance Redressal Committee is Added successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_college_grievance_committee uCommittee = new jntuh_college_grievance_committee();

                        if (uCommittee != null)
                        {
                            uCommittee.id = Committee.id;
                            uCommittee.collegeId = userCollegeID;
                            uCommittee.memberDesignation = Committee.memberDesignation;
                            uCommittee.memberName = Committee.memberName;
                            uCommittee.Email = Committee.Email;
                            uCommittee.Mobile = Committee.Mobile;
                            uCommittee.createdBy = Committee.createdBy;
                            uCommittee.createdOn = Committee.createdOn;
                            uCommittee.updatedBy = userID;
                            uCommittee.updatedOn = DateTime.Now;
                            uCommittee.actualDesignation = Committee.actualDesignationId;
                            db.Entry(uCommittee).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Success"] = "Grievance Redressal Committee is Updated successfully.";
                        }
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_GrievanceRedressalCommitteeData", Committee);
            }
            else
            {
                return View("GrievanceRedressalCommitteeData", Committee);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_grievance_committee committee = db.jntuh_college_grievance_committee.Where(oc => oc.id == id).FirstOrDefault();
            if (committee != null)
            {
                try
                {
                    db.jntuh_college_grievance_committee.Remove(committee);
                    db.SaveChanges();
                    TempData["Success"] = "Grievance Redressal Committee is Deleted successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(committee.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            GrievanceRedressalCommittee grievanceRedressalCommittee = (from gc in db.jntuh_college_grievance_committee
                                                                       join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                                       join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                                       where (gc.id == id)
                                                                       select new GrievanceRedressalCommittee
                                                                       {
                                                                           id = gc.id,
                                                                           collegeId = gc.collegeId,
                                                                           memberDesignation = gc.memberDesignation,
                                                                           designationName = d.Designation,
                                                                           memberName = gc.memberName,
                                                                           my_aspnet_users = gc.my_aspnet_users,
                                                                           my_aspnet_users1 = gc.my_aspnet_users1,
                                                                           actualDesignationId = gc.actualDesignation,
                                                                           actualDesignation = ad.designation,
                                                                           Email = gc.Email,
                                                                           Mobile = gc.Mobile
                                                                       }).FirstOrDefault();
            if (grievanceRedressalCommittee != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_GrievanceRedressalCommitteeDetails", grievanceRedressalCommittee);
                }
                else
                {
                    return View("GrievanceRedressalCommitteeDetails", grievanceRedressalCommittee);
                }
            }
            return View("Index", new { collegeId = Utilities.EncryptString(grievanceRedressalCommittee.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.PreviousYear = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(s => s.academicYear).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("GR") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }

            }

            ViewBag.collegeGRCpath = string.Empty;
            var grcDate = Convert.ToDateTime("2021-06-07 13:44:51");
            var collegegrcNews = db.jntuh_college_news.Where(i => i.collegeId == userCollegeID && i.createdOn == grcDate).ToList();
            if (collegegrcNews.Count > 0)
            {
                ViewBag.collegeGRCpath = collegegrcNews.FirstOrDefault().navigateURL;
            }

            IEnumerable<GrievanceRedressalCommittee> committee = (from gc in db.jntuh_college_grievance_committee
                                                                  join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                                  join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                                  where gc.collegeId == userCollegeID
                                                                  select new GrievanceRedressalCommittee
                                                                  {
                                                                      id = gc.id,
                                                                      //rowNumber = index + 1,
                                                                      collegeId = gc.collegeId,
                                                                      memberDesignation = gc.memberDesignation,
                                                                      memberName = gc.memberName,
                                                                      designationName = d.Designation,
                                                                      my_aspnet_users = gc.my_aspnet_users,
                                                                      my_aspnet_users1 = gc.my_aspnet_users1,
                                                                      actualDesignationId = gc.actualDesignation,
                                                                      actualDesignation = ad.designation,
                                                                      Email = gc.Email,
                                                                      Mobile = gc.Mobile
                                                                  }).OrderBy(r => r.memberName).ToList();

            List<GrievanceRedressalComplaints> complaints = db.jntuh_college_grievance_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new GrievanceRedressalComplaints
                                              {
                                                  id = a.id,
                                                  //rowNumber = index + 1,
                                                  collegeId = a.collegeId,
                                                  complaintReceived = a.complaintReceived,
                                                  actionsTaken = a.actionsTaken,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                              }).OrderBy(r => r.actionsTaken).ToList();

            //Getting Support Document
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Grievance Redressal Proceedings")
                    .Select(e => e.id)
                    .FirstOrDefault();
            ViewBag.SupportDocument =
                db.jntuh_college_enclosures.Where(
                    r => r.collegeID == userCollegeID && r.academicyearId == prAy && r.enclosureId == enclosureId)
                    .Select(s => s.path)
                    .FirstOrDefault();

            ViewBag.Committee = committee;
            ViewBag.Complaints = complaints;
            ViewBag.CommitteeCount = committee.Count();
            ViewBag.ComplaintsCount = complaints.Count();
            return View();
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            IEnumerable<GrievanceRedressalCommittee> committee = (from gc in db.jntuh_college_grievance_committee
                                                                  join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                                  join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                                  where gc.collegeId == userCollegeID
                                                                  select new GrievanceRedressalCommittee
                                                                  {
                                                                      id = gc.id,
                                                                      collegeId = gc.collegeId,
                                                                      memberDesignation = gc.memberDesignation,
                                                                      memberName = gc.memberName,
                                                                      designationName = d.Designation,
                                                                      my_aspnet_users = gc.my_aspnet_users,
                                                                      my_aspnet_users1 = gc.my_aspnet_users1,
                                                                      actualDesignationId = gc.actualDesignation,
                                                                      actualDesignation = ad.designation

                                                                  }).ToList();

            List<GrievanceRedressalComplaints> complaints = db.jntuh_college_grievance_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new GrievanceRedressalComplaints
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  complaintReceived = a.complaintReceived,
                                                  actionsTaken = a.actionsTaken,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                              }).ToList();

            ViewBag.Committee = committee;
            ViewBag.Complaints = complaints;
            ViewBag.CommitteeCount = committee.Count();
            ViewBag.ComplaintsCount = complaints.Count();
            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult GrievanceRedressalSupportfile(HttpPostedFileBase fileUploader, string collegeId)
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
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Grievance Redressal Proceedings")
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
            string filepath = "~/Content/Upload/CollegeEnclosures/GrievanceRedressal";
            if (fileUploader != null)
            {
                if (!Directory.Exists(Server.MapPath(filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(filepath));
                }
                if (college_enclosures != null && college_enclosures.path != null)
                {

                    fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath(filepath),
                        college_enclosures.path));
                    jntuh_college_enclosures.path = college_enclosures.path;
                }
                else
                {
                    string ext = Path.GetExtension(fileUploader.FileName);
                    fileName =
                        db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                        "_GRS_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                    fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath(filepath),
                        fileName));
                    jntuh_college_enclosures.path = fileName;
                }
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
                TempData["Success"] = "Supporting Document Save successfuly";
            }
            else
            {
                college_enclosures.path = college_enclosures.path;
                college_enclosures.updatedBy = userID;
                college_enclosures.updatedOn = DateTime.Now;
                db.Entry(college_enclosures).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Supporting Document Update successfuly";
            }
            return RedirectToAction("Index",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });

        }
    }
}
