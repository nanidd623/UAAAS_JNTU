using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeComplianceController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "College")]
        public ActionResult Index()
        {
            var todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Faculty Complaints - Compliance" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var compphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (compphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            var collegeQuestionnaire = db.jntuh_college_compliance_questionnaire.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            if (collegeQuestionnaire.Count > 0)
            {
                return RedirectToAction("View");
            }

            var compliances = db.jntuh_compliance_questionnaire.Where(i => i.isactive == true).OrderBy(i => i.complaincetype).ToList();
            ViewBag.compliances = compliances;
            var lstCompQues = new List<ComplianceQuestionnaire>();
            foreach (var item in compliances)
            {
                var objCompQues = new ComplianceQuestionnaire()
                {
                    complainceid = item.id,
                    circulardoc = null,
                    supportingdocuments = string.Empty
                };
                lstCompQues.Add(objCompQues);
            }
            return View(lstCompQues);
        }


        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult Create(List<ComplianceQuestionnaire> compQueslist)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID > 0)
            {
                foreach (var item in compQueslist)
                {
                    var objcomplianceQue = new jntuh_college_compliance_questionnaire()
                     {
                         complainceid = item.complainceid,
                         collegeid = userCollegeID,
                         academicyearid = ay0,
                         complaincestatus = item.complaincestatus == "True" ? "Yes" : item.complaincestatus == "False" ? "No" : item.complaincestatus,
                         remarks = item.remarks == null ? "NA" : item.remarks + "%",
                         //remarks1 = item.remarks1,
                         isactive = true,
                         createdby = userID,
                         createdon = DateTime.Now,
                     };

                    if (item.circulardoc != null)
                    {
                        string circularFile = "~/Content/Upload/College/ComplianceQuestionnaire";
                        if (!Directory.Exists(Server.MapPath(circularFile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(circularFile));
                        }
                        var ext = Path.GetExtension(item.circulardoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (item.supportingdocuments == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                item.circulardoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(circularFile), fileName, ext));
                                item.supportingdocuments = string.Format("{0}{1}", fileName, ext);
                                objcomplianceQue.supportingdocuments = item.supportingdocuments;
                            }
                            else
                            {
                                item.circulardoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(circularFile), item.supportingdocuments));
                                objcomplianceQue.supportingdocuments = item.supportingdocuments;
                            }
                        }
                    }
                    db.jntuh_college_compliance_questionnaire.Add(objcomplianceQue);
                    db.SaveChanges();
                    db.Entry(objcomplianceQue).State = EntityState.Detached;
                }
            }
            return RedirectToAction("View");
        }

        [Authorize(Roles = "College")]
        public ActionResult View()
        {
            var todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Faculty Complaints - Compliance" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var compphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (compphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            var masterQuestionnaire = db.jntuh_compliance_questionnaire.Where(i => i.isactive == true).ToList();
            var collegeQuestionnaire = db.jntuh_college_compliance_questionnaire.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var lstQuestionnaire = new List<ComplianceQuestionnaire>();
            foreach (var item in collegeQuestionnaire)
            {
                var objCompQues = new ComplianceQuestionnaire()
                {
                    complainceid = item.complainceid,
                    complaincedescription = masterQuestionnaire.Where(i => i.id == item.complainceid).FirstOrDefault().compliencdescription,
                    complaincestatus = item.complaincestatus,
                    supportingdocuments = item.supportingdocuments,
                    complaincetype = masterQuestionnaire.Where(i => i.id == item.complainceid).FirstOrDefault().complaincetype,
                    remarks = item.remarks,
                };
                lstQuestionnaire.Add(objCompQues);
            }
            return View(lstQuestionnaire);
        }
    }
}
