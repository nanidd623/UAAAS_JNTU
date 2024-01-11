using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports.RTI_Reports
{
    public class RTI_DirectorInformationController : Controller
    {
        //
        // GET: /RTI_DirectorInformation/
        private uaaasDBContext db = new uaaasDBContext();

         [Authorize(Roles = "Admin")]
        public ActionResult Index(int? collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

          ////  int userAddressID = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye.Equals("DIRECTOR")).Select(a => a.id).FirstOrDefault();
            int userDirectorID = db.jntuh_college_principal_director.Where(e => e.collegeId == collegeid && e.type.Equals("DIRECTOR")).Select(e => e.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;

            PrincipalDirector principalDirector = new PrincipalDirector();

            if (collegeid != null)
            {
                
                principalDirector.collegeId = Convert.ToInt32(collegeid);
                principalDirector.type = "DIRECTOR";
                jntuh_college_principal_director jntuh_college_principal = db.jntuh_college_principal_director.Find(userDirectorID);
                if (jntuh_college_principal == null)
                {
                    ViewBag.Norecords = true;
                }
                if (jntuh_college_principal != null && userDirectorID != null)
                {
                    ViewBag.Norecords = false;
                    principalDirector.id = jntuh_college_principal.id;
                    principalDirector.collegeId = jntuh_college_principal.collegeId;
                    principalDirector.firstName = jntuh_college_principal.firstName;
                    principalDirector.lastName = jntuh_college_principal.lastName;
                    principalDirector.surname = jntuh_college_principal.surname;
                    principalDirector.qualification = db.jntuh_qualification.Where(q => q.id == jntuh_college_principal.qualificationId).Select(q => q.qualification).FirstOrDefault();
                    principalDirector.dateOfAppointment = jntuh_college_principal.dateOfAppointment.ToString();
                    principalDirector.dateOfBirth = jntuh_college_principal.dateOfBirth.ToString();
                    principalDirector.fax = jntuh_college_principal.fax;
                    principalDirector.landline = jntuh_college_principal.landline;
                    principalDirector.mobile = jntuh_college_principal.mobile;
                    principalDirector.email = jntuh_college_principal.email;
                    principalDirector.departmentName = db.jntuh_department.Where(d => d.id == jntuh_college_principal.departmentId).Select(d => d.departmentName).FirstOrDefault();
                    principalDirector.phdSubjectName = db.jntuh_phd_subject.Where(p => p.id == jntuh_college_principal.phdId).Select(p => p.phdSubjectName).FirstOrDefault();
                    principalDirector.phdFromUniversity = jntuh_college_principal.phdFromUniversity;
                    principalDirector.phdYear = jntuh_college_principal.phdYear;
                    principalDirector.ratificationPeriodFrom = jntuh_college_principal.ratificationPeriodFrom.ToString();
                    principalDirector.ratificationPeriodTo = jntuh_college_principal.ratificationPeriodTo.ToString();
                }
                jntuh_college jntuhcollege = db.jntuh_college.Find(principalDirector.collegeId);
                if (jntuhcollege != null)
                {
                    principalDirector.collegeCode = jntuhcollege.collegeCode;
                    principalDirector.collegeName = jntuhcollege.collegeName;
                }
            }
            return View(principalDirector);
        }
         public ActionResult RTI_AllDirectorsInformationReport()
         {
             List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => c).ToList();
             List<PrincipalDirector> directorInformationList = new List<PrincipalDirector>();

             foreach (var clgInfo in colleges.ToList())
             {
                 PrincipalDirector principalDirector = new PrincipalDirector();

                 int collegeid = clgInfo.id;

                 if (collegeid != 0)
                 { 
                     int userPrincipalID = db.jntuh_college_principal_director.Where(e => e.collegeId == collegeid && e.type.Equals("DIRECTOR")).Select(e => e.id).FirstOrDefault();

                     principalDirector.collegeId = Convert.ToInt32(collegeid);
                     principalDirector.type = "DIRECTOR";
                     jntuh_college_principal_director jntuh_college_principal = db.jntuh_college_principal_director.Find(userPrincipalID);
                     if (jntuh_college_principal != null)
                     {
                         principalDirector.id = jntuh_college_principal.id;
                         principalDirector.collegeId = jntuh_college_principal.collegeId;
                         principalDirector.firstName = jntuh_college_principal.firstName;
                         principalDirector.lastName = jntuh_college_principal.lastName;
                         principalDirector.surname = jntuh_college_principal.surname;
                         principalDirector.qualification = db.jntuh_qualification.Where(q => q.id == jntuh_college_principal.qualificationId).Select(q => q.qualification).FirstOrDefault();
                         principalDirector.dateOfAppointment = jntuh_college_principal.dateOfAppointment.ToString();
                         principalDirector.dateOfBirth = jntuh_college_principal.dateOfBirth.ToString();
                         principalDirector.fax = jntuh_college_principal.fax;
                         principalDirector.landline = jntuh_college_principal.landline;
                         principalDirector.mobile = jntuh_college_principal.mobile;
                         principalDirector.email = jntuh_college_principal.email;
                         principalDirector.departmentName = db.jntuh_department.Where(d => d.id == jntuh_college_principal.departmentId).Select(d => d.departmentName).FirstOrDefault();
                         principalDirector.phdSubjectName = db.jntuh_phd_subject.Where(p => p.id == jntuh_college_principal.phdId).Select(p => p.phdSubjectName).FirstOrDefault();
                         principalDirector.phdFromUniversity = jntuh_college_principal.phdFromUniversity;
                         principalDirector.phdYear = jntuh_college_principal.phdYear;
                         principalDirector.ratificationPeriodFrom = jntuh_college_principal.ratificationPeriodFrom.ToString();
                         principalDirector.ratificationPeriodTo = jntuh_college_principal.ratificationPeriodTo.ToString();

                         jntuh_college jntuhcollege = db.jntuh_college.Find(principalDirector.collegeId);
                         if (jntuhcollege != null)
                         {
                             principalDirector.collegeCode = jntuhcollege.collegeCode;
                             principalDirector.collegeName = jntuhcollege.collegeName;
                         }
                         directorInformationList.Add(principalDirector);
                     }
                     
                 }
             }
             int Count = directorInformationList.Count();

             if (Count != 0)
             {
                 Response.ClearContent();
                 Response.Buffer = true;
                 Response.AddHeader("content-disposition", "attachment; filename=RTI_AllDirectorssInformation.xls");
                 Response.ContentType = "application//vnd.ms-excel";
                 return PartialView("~/Views/RTI_DirectorInformation/RTI_AllDirectorsInformationReport.cshtml", directorInformationList);
             }
             else
             {
                 ViewBag.Norecords = true;
                 return View("~/Views/RTI_DirectorInformation/Index.cshtml");
             }
         } 
    }
}
