using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class FFCAuditorsSectionController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult FFCAuditorsSection(string group)
        {
            List<jntuh_ffc_auditor> fFCAuditors = new List<jntuh_ffc_auditor>();
            ViewBag.FFCAuditorsGroup = db.jntuh_ffc_external_auditor_groups
                                            .Where(a=>a.isActive==true)
                                            .Select(a => new { Text = a.Group, Value = a.Group })
                                            .Distinct().OrderBy(a => a.Text).ToList();
            if (group != null && group != string.Empty)
            {
                string[] struniversitys = db.jntuh_ffc_external_auditor_groups.Where(g => g.isActive == true && g.Group==group).Select(g => g.University).ToArray();
                fFCAuditors = db.jntuh_ffc_auditor.Where(a =>struniversitys.Contains(a.auditorPlace)).Select(a => a).OrderBy(a=>a.auditorPlace).ThenBy(a=>a.auditorName).ToList();
                ViewBag.AuditoList = true;
                ViewBag.count = fFCAuditors.Count();
                return View("~/Views/Admin/FFCAuditorsSection.cshtml", fFCAuditors);
            }
            else
            {
                ViewBag.AuditoList = false;
            }
            return View("~/Views/Admin/FFCAuditorsSection.cshtml", fFCAuditors);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult FFCAuditorsSection(List<jntuh_ffc_auditor> jntuh_ffc_auditor)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (jntuh_ffc_auditor != null && jntuh_ffc_auditor.Count() > 0)
            {
                foreach (var item in jntuh_ffc_auditor)
                {
                    jntuh_ffc_auditor ffcauditor = new jntuh_ffc_auditor();
                    ffcauditor = db.jntuh_ffc_auditor.Find(item.id);
                    ffcauditor.isActive = item.isActive;
                    ffcauditor.updatedBy = userID;
                    ffcauditor.updatedOn = DateTime.Now;
                    db.Entry(ffcauditor).State = EntityState.Modified;
                    db.SaveChanges();
                }
                TempData["Success"] = "Auditors data updated successfully.";
            }
            else
            {
                TempData["Error"] = "Auditors data not updated.";
            }
            return RedirectToAction("FFCAuditorsSection");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AuditorsCountGroupwise()
        {
            List<jntuh_ffc_external_auditor_groups> auditorgroups = new List<Models.jntuh_ffc_external_auditor_groups>();
            auditorgroups=db.jntuh_ffc_external_auditor_groups.Where(a => a.isActive == true).Select(a => a).ToList();
            return View("~/Views/Admin/AuditorsCountGroupwise.cshtml", auditorgroups);
        }

    }
}
