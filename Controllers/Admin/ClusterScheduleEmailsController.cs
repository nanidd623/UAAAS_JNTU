using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class ClusterScheduleEmailsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        //Commented on 18-06-2018 by Narayana Reddy
        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public ActionResult ClusterScheduleSendEmails()
        //{
        //    ClusterScheduleEmails clusterScheduleEmails = new ClusterScheduleEmails();
        //    ViewBag.Cluster = db.college_clusters.Select(c => c.clusterName).Distinct()
        //                        .Select(c => new
        //                        {
        //                            Value = c,
        //                            Text = c
        //                        }).OrderBy(c => c).ToList();
        //    return View("~/Views/Admin/ClusterScheduleSendEmails.cshtml", clusterScheduleEmails);
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public ActionResult ClusterScheduleSendEmails(ClusterScheduleEmails clusterScheduleEmails)
        //{
        //    ViewBag.Cluster = db.college_clusters.Select(c => c.clusterName).Distinct()
        //                        .Select(c => new
        //                        {
        //                            Value = c,
        //                            Text = c
        //                        }).OrderBy(c => c).ToList();

        //    if (ModelState.IsValid)
        //    {
        //        int?[] collegeIDs = db.college_clusters.Where(c => c.clusterName == clusterScheduleEmails.cluster).Select(c => c.collegeId).ToArray();
        //        string strCc = ConfigurationManager.AppSettings["EmailCC"].ToString();
        //        string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();
        //        foreach (var collegeId in collegeIDs)
        //        {
        //            jntuh_ffc_cluster_emails jntuh_ffc_cluster_emails = new jntuh_ffc_cluster_emails();
        //            string stremails = string.Empty;
        //            string strmobilenos = string.Empty;
        //            var collegeAddress = db.jntuh_address.Where(a => a.collegeId == collegeId).Select(a => a).ToList();
        //            foreach (var item in collegeAddress)
        //            {
        //                strmobilenos += item.mobile + ",";
        //                stremails += item.email + ",";
        //            }
        //            strmobilenos = strmobilenos.Substring(0, strmobilenos.Length - 1);
        //            stremails = stremails.Substring(0, stremails.Length - 1);
        //            #region sendingEmail
        //            string strmessage = clusterScheduleEmails.emailBody;
        //            clusterScheduleEmails.collegeId = (int)collegeId;
        //            var college = db.jntuh_college.Find(collegeId);
        //            string collegeName = college.collegeName.Replace("&", "and");
        //            string collegeCode = college.collegeCode;

        //            strmessage = strmessage.Replace("##COLLEGE_NAME##", collegeName);
        //            strmessage = strmessage.Replace("##COLLEGE_CODE##", collegeCode);
        //            //send emails
        //            IUserMailer mailer = new UserMailer();
        //            mailer.ClusterEmails(stremails, strCc, strBcc, clusterScheduleEmails.emailSubject, strmessage).SendAsync();

        //            //send sms
        //            string strsmstext = clusterScheduleEmails.smsText;
        //            strsmstext = strsmstext.Replace("##COLLEGE_NAME##", collegeName);
        //            strsmstext = strsmstext.Replace("##COLLEGE_CODE##", collegeCode);
        //            strmobilenos = strmobilenos + ",8143528998,9493666388";
        //            bool pStatus = UAAAS.Models.Utilities.SendSms(strmobilenos, strsmstext);
        //            #endregion


        //            #region clusterEmails
        //            jntuh_ffc_cluster_emails.collegeId = (int)collegeId;
        //            jntuh_ffc_cluster_emails.emailSubject = clusterScheduleEmails.emailSubject;
        //            jntuh_ffc_cluster_emails.emailBody = strmessage;
        //            jntuh_ffc_cluster_emails.emailTo = stremails;
        //            jntuh_ffc_cluster_emails.smsText = strsmstext;
        //            jntuh_ffc_cluster_emails.smsTo = strmobilenos;
        //            jntuh_ffc_cluster_emails.createdOn = DateTime.Now;
        //            jntuh_ffc_cluster_emails.createdBy = 1;
        //            db.jntuh_ffc_cluster_emails.Add(jntuh_ffc_cluster_emails);
        //            db.SaveChanges();
        //            #endregion

        //            #region collegeNews
        //            jntuh_college_news jntuh_college_news = new jntuh_college_news();
        //            jntuh_college_news.collegeId = (int)collegeId;
        //            jntuh_college_news.title = strsmstext;
        //            jntuh_college_news.startDate = DateTime.Now.Date;
        //            jntuh_college_news.endDate = DateTime.Now.Date.AddDays(10);
        //            jntuh_college_news.isActive = true;
        //            jntuh_college_news.isLatest = true;
        //            jntuh_college_news.createdBy = 1;
        //            jntuh_college_news.createdOn = DateTime.Now;
        //            db.jntuh_college_news.Add(jntuh_college_news);
        //            db.SaveChanges();
        //            #endregion
        //            TempData["Success"] = "Mail and SMS sent successfully";
        //        }
        //    }
        //    return RedirectToAction("ClusterScheduleSendEmails");
        //}

    }
}
