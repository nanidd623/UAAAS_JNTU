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

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeGoverningbodyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /CollegeGoverningbody/

        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Index()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_college = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();
            List<Governingbody> governingbodylist = new List<Governingbody>();
            var governingbodydesignations = db.jntuh_governingbodydesignations.AsNoTracking().Select(s => s).ToList();
            var governinglist =
                db.jntuh_college_governingbody.Where(r => r.collegeid == userCollegeID).Select(s => s).ToList();
            List<Governingbody> gbodylist = new List<Governingbody>();
            if (governinglist.Count != 0)
            {
                return RedirectToAction("GoverningbodyView");
            }
            foreach (var item in governingbodydesignations)
            {
                Governingbody gbody = new Governingbody();
                gbody.MemberDesignationId = (short)item.id;
                if (item.designation == "Member Secretary [Principal(ex-officio)]" && jntuh_college != null)
                {
                    //College Code 7Z don't have principal so we freaching data as name of director of this college by colege id
                    if (jntuh_college.id == 39)
                    {
                        var directordetails =
                       db.jntuh_college_principal_director.Where(p => p.collegeId == userCollegeID && p.type == "DIRECTOR")
                           .Select(s => s)
                           .FirstOrDefault();
                        if (directordetails != null)
                        {

                            gbody.ParentOrganizationwhereworking = jntuh_college.collegeName;
                            gbody.DesignationofthememberwhereworkingatparentOrganization = "Director";
                            gbody.NameoftheGoverningBodyMember = directordetails.firstName + " " + directordetails.lastName +
                                                             " " + directordetails.surname;
                        }
                    }
                    else
                    {
                        var principalreg =
                       db.jntuh_college_principal_registered.Where(p => p.collegeId == userCollegeID)
                           .Select(s => s.RegistrationNumber)
                           .FirstOrDefault();
                        if (principalreg != null)
                        {
                            var principalname =
                                db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == principalreg)
                                    .Select(s => new { s.FirstName, s.MiddleName, s.LastName })
                                    .FirstOrDefault();

                            if (principalname != null)
                            {
                                gbody.ParentOrganizationwhereworking = jntuh_college.collegeName;
                                gbody.DesignationofthememberwhereworkingatparentOrganization = "Principal";
                                gbody.NameoftheGoverningBodyMember = principalname.FirstName + " " + principalname.MiddleName +
                                                                 " " + principalname.LastName;
                            }
                        }
                        else
                        {
                            TempData["ERROR"] = "without the appointment of Principal you cann't proceed further.";
                        }
                    }
                }
                gbody.GoverningBodyMemberDesignation = item.designation;
                gbodylist.Add(gbody);
            }

            List<string> designationtype = new List<string>();
            var designationsdata = db.jntuh_college_governingbody.Select(r => r.organizationdesignation).Distinct().ToList();
            designationtype = designationsdata;
            ViewBag.Collegeslist = db.jntuh_college.Select(s => s.collegeName).ToList();
            designationtype.Add("Professor");
            ViewBag.Designationslist = designationtype;

            return View(gbodylist);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Index(IList<Governingbody> listobj)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var jntuhCollege = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();

            var requriedlist = listobj.Take(11).ToList();
            foreach (var req in requriedlist)
            {
                if (req.SupportingDocument == null || req.NameoftheGoverningBodyMember == null)
                {
                    TempData["ERROR"] = "Governing Body Committee Composition not as per the Regulations.";
                    return RedirectToAction("Index");
                }
            }

            if (listobj.Count != 0)
            {
                foreach (var item in listobj)
                {
                    if (item.NameoftheGoverningBodyMember != null && item.SupportingDocument != null)
                    {
                        var governingbodypath = "~/Content/Upload/College/GoverningBody";
                        if (!Directory.Exists(Server.MapPath(governingbodypath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(governingbodypath));
                        }
                        var ext = Path.GetExtension(item.SupportingDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string filename = jntuhCollege.collegeCode + "-" + item.MemberDesignationId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_GB";
                            item.SupportingDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(governingbodypath), filename, ext));
                            item.SupportingDocumentfile = string.Format("{0}{1}", filename, ext);
                        }
                        //jntuh_college_governingbody updateGoverningbody = db.jntuh_college_governingbody.Where(r => r.collegeid == userCollegeID && r.memberdesignation == item.MemberDesignationId).Select(s => s).FirstOrDefault();

                        //if (updateGoverningbody != null)
                        //{
                        //    updateGoverningbody.academicyearid = ay0;
                        //    updateGoverningbody.collegeid = userCollegeID;
                        //    updateGoverningbody.nameofthemember = item.NameoftheGoverningBodyMember;
                        //    updateGoverningbody.memberdesignation = item.MemberDesignationId;
                        //    updateGoverningbody.dateofappointment = UAAAS.Models.Utilities.DDMMYY2MMDDYY(item.DateofappointmentasGoverningBodymember);
                        //    updateGoverningbody.organizationworking = item.ParentOrganizationwhereworking;
                        //    updateGoverningbody.organizationdesignation = item.DesignationofthememberwhereworkingatparentOrganization;
                        //    updateGoverningbody.supportingdocument = item.SupportingDocumentfile;
                        //    //newGoverningbody.createdby = userID;
                        //    //newGoverningbody.createdon = DateTime.Now;
                        //    updateGoverningbody.updatedon = DateTime.Now;
                        //    updateGoverningbody.updatedby = userID;
                        //    db.Entry(updateGoverningbody).State = EntityState.Modified;
                        //    db.SaveChanges();
                        //    TempData["Success"] = "Data Updated Successfully.";
                        //}
                        //else
                        //{
                        jntuh_college_governingbody newGoverningbody = new jntuh_college_governingbody();

                        newGoverningbody.academicyearid = ay0;
                        newGoverningbody.collegeid = userCollegeID;
                        newGoverningbody.nameofthemember = item.NameoftheGoverningBodyMember;
                        newGoverningbody.memberdesignation = item.MemberDesignationId;
                        newGoverningbody.dateofappointment = UAAAS.Models.Utilities.DDMMYY2MMDDYY(item.DateofappointmentasGoverningBodymember);
                        newGoverningbody.organizationworking = item.ParentOrganizationwhereworking;
                        newGoverningbody.organizationdesignation = item.DesignationofthememberwhereworkingatparentOrganization;
                        newGoverningbody.supportingdocument = item.SupportingDocumentfile;
                        newGoverningbody.createdby = userID;
                        newGoverningbody.createdon = DateTime.Now;
                        db.jntuh_college_governingbody.Add(newGoverningbody);
                        db.SaveChanges();
                        TempData["Success"] = "Data Saved Successfully.";
                        //}                        
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult GoverningbodyView()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_college = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();
            List<Governingbody> governingbodylist = new List<Governingbody>();
            var governingbodydesignations = db.jntuh_governingbodydesignations.AsNoTracking().Select(s => s).ToList();
            var governinglist =
                db.jntuh_college_governingbody.Where(r => r.collegeid == userCollegeID).Select(s => s).ToList();
            if (governinglist.Count == 0)
            {
                return RedirectToAction("Index");
            }
            List<Governingbody> gbodylist = new List<Governingbody>();
            foreach (var item in governingbodydesignations)
            {
                Governingbody gbody = new Governingbody();
                gbody.MemberDesignationId = (short)item.id;
                if (item.designation == "Member Secretary [Principal(ex-officio)]" && jntuh_college != null)
                {
                    //College Code 7Z don't have principal so we freaching data as name of director of this college by colege id
                    if (jntuh_college.id == 39)
                    {
                        var directordetails =
                       db.jntuh_college_principal_director.Where(p => p.collegeId == userCollegeID && p.type == "DIRECTOR")
                           .Select(s => s)
                           .FirstOrDefault();
                        if (directordetails != null)
                        {

                            gbody.ParentOrganizationwhereworking = jntuh_college.collegeName;
                            gbody.DesignationofthememberwhereworkingatparentOrganization = "Director";
                            gbody.NameoftheGoverningBodyMember = directordetails.firstName + " " + directordetails.lastName +
                                                             " " + directordetails.surname;
                        }
                    }
                    else
                    {
                        var principalreg =
                       db.jntuh_college_principal_registered.Where(p => p.collegeId == userCollegeID)
                           .Select(s => s.RegistrationNumber)
                           .FirstOrDefault();
                        if (principalreg != null)
                        {
                            var principalname =
                                db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == principalreg)
                                    .Select(s => new { s.FirstName, s.MiddleName, s.LastName })
                                    .FirstOrDefault();

                            if (principalname != null)
                            {
                                gbody.ParentOrganizationwhereworking = jntuh_college.collegeName;
                                gbody.DesignationofthememberwhereworkingatparentOrganization = "Principal";
                                gbody.NameoftheGoverningBodyMember = principalname.FirstName + " " + principalname.MiddleName +
                                                                 " " + principalname.LastName;
                            }
                        }
                        else
                        {
                            TempData["ERROR"] = "without the appointment of Principal you cann't proceed further.";
                        }
                    }
                }
                gbody.GoverningBodyMemberDesignation = item.designation;

                var data =
                    governinglist.Where(r => r.memberdesignation == gbody.MemberDesignationId)
                        .Select(s => s)
                        .FirstOrDefault();
                if (data != null && gbody.MemberDesignationId != 11)//not checking Member Secretary [Principal(ex-officio)]
                {
                    gbody.Academicyearid = data.academicyearid;
                    gbody.Collegeid = data.collegeid;
                    gbody.NameoftheGoverningBodyMember = data.nameofthemember;
                    gbody.ParentOrganizationwhereworking = data.organizationworking;
                    gbody.DesignationofthememberwhereworkingatparentOrganization = data.organizationdesignation;
                    //DateTime date = Convert.ToDateTime(data.dateofappointment);
                    gbody.DateofappointmentasGoverningBodymember = data.dateofappointment.ToString().Split(' ')[0];
                    //gbody.DateofappointmentasGoverningBodymember = date.ToString("dd/MM/yyyy").Split(' ')[0];
                    //gbody.DateofappointmentasGoverningBodymember =
                    //    gbody.DateofappointmentasGoverningBodymember.ToString("dd-mm-yyyy");
                    gbody.SupportingDocumentfile = data.supportingdocument;
                    gbody.SupportingDocumentfile1 = data.supportingdocument1;

                }
                //var requriedlist = governinglist.Take(11).ToList();
                //foreach (var req in requriedlist)
                //{
                //    if (req.supportingdocument == null || req.nameofthemember == null)
                //    {
                //        TempData["Success"] = "file cann't be empty.";
                //        return RedirectToAction("Index");
                //    }
                //}
                gbodylist.Add(gbody);
            }
            return View(gbodylist);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult GoverningbodyEdit()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_college = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();
            List<Governingbody> governingbodylist = new List<Governingbody>();
            var governingbodydesignations = db.jntuh_governingbodydesignations.AsNoTracking().Select(s => s).ToList();
            var governinglist =
                db.jntuh_college_governingbody.Where(r => r.collegeid == userCollegeID).Select(s => s).ToList();
            List<Governingbody> gbodylist = new List<Governingbody>();
            foreach (var item in governingbodydesignations)
            {
                Governingbody gbody = new Governingbody();
                gbody.MemberDesignationId = (short)item.id;
                if (item.designation == "Member Secretary [Principal(ex-officio)]" && jntuh_college != null)
                {
                    //College Code 7Z don't have principal so we freaching data as name of director of this college by colege id
                    if (jntuh_college.id == 39)
                    {
                        var directordetails =
                       db.jntuh_college_principal_director.Where(p => p.collegeId == userCollegeID && p.type == "DIRECTOR")
                           .Select(s => s)
                           .FirstOrDefault();
                        if (directordetails != null)
                        {

                            gbody.ParentOrganizationwhereworking = jntuh_college.collegeName;
                            gbody.DesignationofthememberwhereworkingatparentOrganization = "Director";
                            gbody.NameoftheGoverningBodyMember = directordetails.firstName + " " + directordetails.lastName +
                                                             " " + directordetails.surname;
                        }
                    }
                    else
                    {
                        var principalreg =
                       db.jntuh_college_principal_registered.Where(p => p.collegeId == userCollegeID)
                           .Select(s => s.RegistrationNumber)
                           .FirstOrDefault();
                        if (principalreg != null)
                        {
                            var principalname =
                                db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == principalreg)
                                    .Select(s => new { s.FirstName, s.MiddleName, s.LastName })
                                    .FirstOrDefault();

                            if (principalname != null)
                            {
                                gbody.ParentOrganizationwhereworking = jntuh_college.collegeName;
                                gbody.DesignationofthememberwhereworkingatparentOrganization = "Principal";
                                gbody.NameoftheGoverningBodyMember = principalname.FirstName + " " + principalname.MiddleName +
                                                                 " " + principalname.LastName;
                            }
                        }
                        else
                        {
                            TempData["ERROR"] = "without the appointment of Principal you cann't proceed further.";
                        }
                    }
                }
                gbody.GoverningBodyMemberDesignation = item.designation;

                var data =
                    governinglist.Where(r => r.memberdesignation == gbody.MemberDesignationId)
                        .Select(s => s)
                        .FirstOrDefault();
                if (data != null && gbody.MemberDesignationId != 11) // not checking Member Secretary [Principal(ex-officio)]
                {
                    gbody.Id = data.id;
                    gbody.Academicyearid = data.academicyearid;
                    gbody.Collegeid = data.collegeid;
                    gbody.NameoftheGoverningBodyMember = data.nameofthemember;
                    gbody.ParentOrganizationwhereworking = data.organizationworking;
                    gbody.DesignationofthememberwhereworkingatparentOrganization = data.organizationdesignation;
                    DateTime date = Convert.ToDateTime(data.dateofappointment);
                    //gbody.DateofappointmentasGoverningBodymember = data.dateofappointment.ToString().Split(' ')[0];
                    gbody.DateofappointmentasGoverningBodymember = date.ToString("dd/MM/yyyy").Split(' ')[0];
                    //gbody.DateofappointmentasGoverningBodymember =
                    //    gbody.DateofappointmentasGoverningBodymember.ToString("dd-mm-yyyy");
                    gbody.SupportingDocumentfile = data.supportingdocument;
                    gbody.SupportingDocumentfile1 = data.supportingdocument1;

                }

                gbodylist.Add(gbody);
            }
            List<string> designationtype = new List<string>();
            var designationsdata = db.jntuh_college_governingbody.Select(r => r.organizationdesignation).Distinct().ToList();
            designationtype = designationsdata;
            ViewBag.Collegeslist = db.jntuh_college.Select(s => s.collegeName).ToList();
            designationtype.Add("Professor");
            ViewBag.Designationslist = designationtype;

            return View(gbodylist);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult GoverningbodyEdit(IList<Governingbody> editlist)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var checkdbdata =
               db.jntuh_college_governingbody.Where(r => r.collegeid == userCollegeID).Select(s => s).ToList();

            //if (checkdbdata.Count != 0)
            //{
            //    return RedirectToAction("GoverningbodyView");
            //}
            List<Governingbody> governingbodylist = new List<Governingbody>();
            var governingbodydesignations = db.jntuh_governingbodydesignations.AsNoTracking().Select(s => s).ToList();
            var governinglist =
                db.jntuh_college_governingbody.Where(r => r.collegeid == userCollegeID).Select(s => s).ToList();
            var requriedlist = editlist.Take(11).ToList();
            foreach (var req in requriedlist)
            {
                if (req.DateofappointmentasGoverningBodymember == null || req.NameoftheGoverningBodyMember == null)
                {
                    TempData["ERROR"] = "Governing Body Committee Composition not as per the Regulations.";
                    return RedirectToAction("Index");
                }
            }
            var jntuhCollege = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();
            List<Governingbody> gbodylist = new List<Governingbody>();
            if (editlist.Count != 0)
            {
                foreach (var item in editlist)
                {
                    if (item.SupportingDocument != null)
                    {
                        var governingbodypath = "~/Content/Upload/College/GoverningBody";
                        if (!Directory.Exists(Server.MapPath(governingbodypath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(governingbodypath));
                        }
                        var ext = Path.GetExtension(item.SupportingDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            if (item.SupportingDocumentfile != null)
                            {
                                item.SupportingDocument.SaveAs(string.Format("{0}/{1}",
                                   Server.MapPath(governingbodypath), item.SupportingDocumentfile));
                                item.SupportingDocumentfile = item.SupportingDocumentfile;
                            }
                            else
                            {
                                string filename = jntuhCollege.collegeCode + "-" + item.MemberDesignationId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_GB";
                                item.SupportingDocument.SaveAs(string.Format("{0}/{1}{2}",
                                    Server.MapPath(governingbodypath), filename, ext));
                                item.SupportingDocumentfile = string.Format("{0}{1}", filename, ext);
                            }
                        }

                    }
                    if (item.SupportingDocument1 != null)
                    {
                        var governingbodypath = "~/Content/Upload/College/GoverningBodypics";
                        if (!Directory.Exists(Server.MapPath(governingbodypath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(governingbodypath));
                        }
                        var photoext = Path.GetExtension(item.SupportingDocument1.FileName);
                        if (photoext.ToUpper().Equals(".jpg") || photoext.ToUpper().Equals(".JPG") || photoext.ToUpper().Equals(".jpeg") || photoext.ToUpper().Equals(".JPEG"))
                        {
                            if (item.SupportingDocumentfile1 != null)
                            {
                                item.SupportingDocument1.SaveAs(string.Format("{0}/{1}",
                                   Server.MapPath(governingbodypath), item.SupportingDocumentfile1));
                                item.SupportingDocumentfile1 = item.SupportingDocumentfile1;
                            }
                            else
                            {
                                string filename = jntuhCollege.collegeCode + "-" + item.MemberDesignationId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_GB";
                                item.SupportingDocument1.SaveAs(string.Format("{0}/{1}{2}",
                                    Server.MapPath(governingbodypath), filename, photoext));
                                item.SupportingDocumentfile1 = string.Format("{0}{1}", filename, photoext);
                            }
                        }
                    }
                    short degid = (short)item.MemberDesignationId;
                    jntuh_college_governingbody newGoverningbody = governinglist.Where(r => r.collegeid == userCollegeID && r.memberdesignation == degid && r.id == item.Id).Select(s => s).FirstOrDefault();
                    if (newGoverningbody != null)
                    {
                        //newGoverningbody.academicyearid = ay0;
                        //newGoverningbody.collegeid = userCollegeID;
                        newGoverningbody.nameofthemember = item.NameoftheGoverningBodyMember;
                        //newGoverningbody.memberdesignation = item.MemberDesignationId;
                        newGoverningbody.dateofappointment = UAAAS.Models.Utilities.DDMMYY2MMDDYY(item.DateofappointmentasGoverningBodymember);
                        newGoverningbody.organizationworking = item.ParentOrganizationwhereworking;
                        newGoverningbody.organizationdesignation = item.DesignationofthememberwhereworkingatparentOrganization;
                        newGoverningbody.supportingdocument = item.SupportingDocumentfile;
                        newGoverningbody.supportingdocument1 = item.SupportingDocumentfile1;
                        //newGoverningbody.createdby = userID;
                        //newGoverningbody.createdon = DateTime.Now;
                        newGoverningbody.updatedon = DateTime.Now;
                        newGoverningbody.updatedby = userID;
                        db.Entry(newGoverningbody).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Data Updated Successfully.";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.NameoftheGoverningBodyMember))
                        {
                            jntuh_college_governingbody Governingbody = new jntuh_college_governingbody();

                            Governingbody.academicyearid = ay0;
                            Governingbody.collegeid = userCollegeID;
                            Governingbody.nameofthemember = item.NameoftheGoverningBodyMember;
                            Governingbody.memberdesignation = item.MemberDesignationId;
                            Governingbody.dateofappointment = UAAAS.Models.Utilities.DDMMYY2MMDDYY(item.DateofappointmentasGoverningBodymember);
                            Governingbody.organizationworking = item.ParentOrganizationwhereworking;
                            Governingbody.organizationdesignation = item.DesignationofthememberwhereworkingatparentOrganization;
                            Governingbody.supportingdocument = item.SupportingDocumentfile;
                            Governingbody.supportingdocument1 = item.SupportingDocumentfile1;
                            Governingbody.createdby = userID;
                            Governingbody.createdon = DateTime.Now;
                            db.jntuh_college_governingbody.Add(Governingbody);
                            db.SaveChanges();
                        }

                    }
                }
            }
            return RedirectToAction("GoverningbodyView");
        }
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]

        public ActionResult Delete(string gobid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int gobID = Convert.ToInt32(Utilities.DecryptString(gobid, WebConfigurationManager.AppSettings["CryptoKey"]));

            var jntuh_college_governingbody = db.jntuh_college_governingbody.Where(a => a.id == gobID).FirstOrDefault();
            db.Entry(jntuh_college_governingbody).State = EntityState.Deleted;
            db.SaveChanges();

            TempData["Success"] = "Deleted successfully";
            return RedirectToAction("GoverningbodyEdit", "CollegeGoverningbody");

            //return RedirectToAction("GoverningbodyEdit", "CollegeGoverningbody", new
            //{
            //    collegeId = collegeId,
            //    courtCaseId = ""
            //});
        }

        #region Admin View

        [Authorize(Roles = "Admin")]
        public ActionResult CollegesGverningbodybyAdmin(int? collegeid)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var governinglist =
                    db.jntuh_college_governingbody.Select(s => s).ToList();
            var collegeids = governinglist.Distinct().Select(s => s.collegeid).ToArray();
            //var cids=
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true && collegeids.Contains(c.id)).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.collegeid = collegeid;
            List<Governingbody> gbodylist = new List<Governingbody>();
            if (collegeid != 0 && collegeid != null)
            {
                List<Governingbody> governingbodylist = new List<Governingbody>();
                var governingbodydesignations = db.jntuh_governingbodydesignations.AsNoTracking().Select(s => s).ToList();


                foreach (var item in governingbodydesignations)
                {
                    Governingbody gbody = new Governingbody();
                    gbody.MemberDesignationId = (short)item.id;
                    gbody.GoverningBodyMemberDesignation = item.designation;
                    var data =
                        governinglist.Where(r => r.memberdesignation == gbody.MemberDesignationId && r.collegeid == collegeid)
                            .Select(s => s)
                            .FirstOrDefault();
                    if (data != null && gbody.MemberDesignationId != 11) // not checking Member Secretary [Principal(ex-officio)]
                    {

                        gbody.Academicyearid = data.academicyearid;
                        gbody.Collegeid = data.collegeid;
                        gbody.NameoftheGoverningBodyMember = data.nameofthemember;
                        gbody.ParentOrganizationwhereworking = data.organizationworking;
                        gbody.DesignationofthememberwhereworkingatparentOrganization = data.organizationdesignation;
                        // DateTime date = Convert.ToDateTime(data.dateofappointment);
                        gbody.DateofappointmentasGoverningBodymember = data.dateofappointment.ToString().Split(' ')[0];
                        //gbody.DateofappointmentasGoverningBodymember = date.ToString("dd/MM/yyyy").Split(' ')[0];
                        //gbody.DateofappointmentasGoverningBodymember =
                        //    gbody.DateofappointmentasGoverningBodymember.ToString("dd-mm-yyyy");
                        gbody.SupportingDocumentfile = data.supportingdocument;
                    }
                    gbodylist.Add(gbody);
                }
                ViewBag.GoverningBodyMeetings = getGoverningmeetings(collegeid);
            }
            return View(gbodylist);
        }
        private List<Governingmeeting> getGoverningmeetings(int? collegeId)
        {
            List<Governingmeeting> Minitesdata = new List<Governingmeeting>();
            var dbdata = db.jntuh_college_governingmeeting.Where(r => r.collegeid == collegeId).Select(s => s).ToList();
            foreach (var item in dbdata)
            {
                Governingmeeting minites = new Governingmeeting();
                minites.Id = item.id;
                minites.Academicyearid = item.academicyearid;
                minites.Collegeid = item.collegeid;
                minites.Dateofmetting = item.dateofmeeting.ToString().Split(' ')[0];
                minites.Minutescopyfile = item.minutescopy;
                minites.Remarks = item.remarks;
                Minitesdata.Add(minites);
            }
            return Minitesdata.OrderByDescending(s => s.Id).ThenBy(r => r.Id).ToList();
        }

        public ActionResult GoverningBodyExport()
        {
            var jntuh_college = db.jntuh_college.Select(s => s).ToList();
            var jntuh_governingbody = db.jntuh_college_governingbody.Where(r => r.memberdesignation == 10).Select(s => s).ToList();
            List<Governingbody> GoverningbodyList = new List<Governingbody>();
            foreach (var item in jntuh_governingbody)
            {
                Governingbody newitem = new Governingbody();
                newitem.Collegecode =
                    jntuh_college.Where(r => r.id == item.collegeid).Select(s => s.collegeCode).FirstOrDefault();
                newitem.Collegename =
                    jntuh_college.Where(r => r.id == item.collegeid).Select(s => s.collegeName).FirstOrDefault();
                newitem.NameoftheGoverningBodyMember = item.nameofthemember;
                newitem.GoverningBodyMemberDesignation = "University Nominee";
                newitem.ParentOrganizationwhereworking = item.organizationworking;
                newitem.DesignationofthememberwhereworkingatparentOrganization = item.organizationdesignation;
                newitem.DateofappointmentasGoverningBodymember = item.dateofappointment.ToString().Split(' ')[0];
                newitem.SupportingDocumentfile = "http://jntuhaac.in/Content/Upload/College/GoverningBody/" + item.supportingdocument;
                GoverningbodyList.Add(newitem);
            }
            string ReportHeader = string.Empty;

            ReportHeader = "Governing Bodys.xls";

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/CollegeGoverningbody/GoverningBodyExport.cshtml", GoverningbodyList);
        }
        #endregion
    }
}
