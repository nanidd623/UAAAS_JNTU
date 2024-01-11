using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class PrincipalRatificationController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //Commented on 14-06-2018 by Narayana Reddy
        //    [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //    public ActionResult Index()
        //    {
        //        //DEO Submitted colleges Ids
        //        int[] SubmittedCollegesId = db.jntuh_college_edit_status.Where(editStatus => editStatus.IsCollegeEditable == false).Select(editStatus => editStatus.collegeId).ToArray();
        //        int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true && SubmittedCollegesId.Contains(c.id)).OrderBy(c => c.collegeName).Select(c => c.id).ToArray();

        //        var principals = db.jntuh_college
        //                           .GroupJoin(db.jntuh_college_principal_ratified,
        //                           p => p.id,
        //                           c => c.collegeId,
        //                           (p, g) => g.Select(c => new PrincipalRatification
        //                           {
        //                               collegeId = p.id,
        //                               collegeCode = p.collegeCode,
        //                               collegeName = p.collegeName,
        //                               isRatified = c.isRatified
        //                           })
        //                           .DefaultIfEmpty(new PrincipalRatification
        //                           {
        //                               collegeId = p.id,
        //                               collegeCode = p.collegeCode,
        //                               collegeName = p.collegeName,
        //                               isRatified = false
        //                           }))
        //                           .Select(g => g).ToList();

        //        List<PrincipalRatification> lst = new List<PrincipalRatification>();

        //        //for (int i = 0; i < principals.Count(); i++)
        //        //{
        //        //    foreach (var item in principals[i])
        //        //    {
        //        //        lst.Add(item);
        //        //    }
        //        //}
        //        principals.ForEach(p =>
        //        {
        //            p.ToList().ForEach(r =>
        //            {
        //                if (collegeIDs.Contains(r.collegeId))
        //                {
        //                    lst.Add(r);
        //                }
        //            });
        //        });

        //        return View(lst);
        //    }

        //    [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //    [HttpPost]
        //    public ActionResult Save(List<PrincipalRatification> newPrincipals)
        //    {
        //        int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

        //        newPrincipals.ForEach(p =>
        //        {
        //            jntuh_college_principal_ratified rPrincipal = db.jntuh_college_principal_ratified.Where(r => r.collegeId == p.collegeId).Select(r => r).FirstOrDefault();

        //            if (rPrincipal != null)
        //            {
        //                rPrincipal.isRatified = p.isRatified;
        //                rPrincipal.updatedBy = userID;
        //                rPrincipal.updatedOn = DateTime.Now;
        //                db.Entry(rPrincipal).State = EntityState.Modified;
        //            }
        //            else
        //            {
        //                rPrincipal = new jntuh_college_principal_ratified();
        //                rPrincipal.collegeId = p.collegeId;
        //                rPrincipal.isRatified = p.isRatified;
        //                rPrincipal.isActive = true;
        //                rPrincipal.createdBy = userID;
        //                rPrincipal.createdOn = DateTime.Now;
        //                db.jntuh_college_principal_ratified.Add(rPrincipal);
        //            }
        //        });

        //        db.SaveChanges();
        //        TempData["Success"] = "Data submitted successfully";
        //        return RedirectToAction("Index");
        //    }

        //}

        public class PrincipalRatification
        {
            public int collegeId { get; set; }
            public string collegeCode { get; set; }
            public string collegeName { get; set; }
            public bool isRatified { get; set; }
        }
    }
}
