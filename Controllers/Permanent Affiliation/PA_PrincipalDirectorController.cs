using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Controllers.College;
using UAAAS.Models;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class PA_PrincipalDirectorController : BaseController
    {
        private readonly uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult Index(string id)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var userId = Convert.ToInt32(userdata.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId <= 0)
            {
                userCollegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var status = GetPageEditableStatus(userCollegeId);
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PPD") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                ViewBag.IsEditable = isPageEditable;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            var academicYears = db.jntuh_academic_year.Where(i => i.actualYear > 2013).Select(s => s).OrderByDescending(i => i.id).ToList();
            var principalDirector = new List<PrincipalDirector>();
            var principalpreviousdetails = db.jntuh_college_facultytracking.AsNoTracking().Where(i => i.collegeId == userCollegeId && i.ActionType == 1 && i.FacultyType == "Principal" && i.isActive == true && i.FacultyStatus == "S").ToList();
            var prinipalReglst = principalpreviousdetails.Select(i => i.RegistrationNumber).ToArray();
            var regdetails = db.jntuh_registered_faculty.AsNoTracking().Where(i => prinipalReglst.Contains(i.RegistrationNumber)).ToList();
            foreach (var pregdet in principalpreviousdetails)
            {
                var jntuhCollegeFacultytracking = principalpreviousdetails.FirstOrDefault(i => i.Id == pregdet.Id);
                var jntuhAcademicYear = academicYears.FirstOrDefault(i => jntuhCollegeFacultytracking != null && i.id == jntuhCollegeFacultytracking.academicYearId);
                if (jntuhAcademicYear != null)
                {
                    var jntuhRegisteredFaculty = regdetails.FirstOrDefault(i => i.RegistrationNumber == pregdet.RegistrationNumber);
                    if (jntuhRegisteredFaculty == null) continue;
                    var objprindetails = new PrincipalDirector
                    {
                        id = pregdet.Id,
                        AcademicYear = jntuhCollegeFacultytracking != null ? jntuhAcademicYear.academicYear : "",
                        RegistrationNumber = jntuhCollegeFacultytracking != null ? jntuhCollegeFacultytracking.RegistrationNumber : "",
                        firstName = jntuhRegisteredFaculty.FirstName,
                        lastName = jntuhRegisteredFaculty.LastName,
                        PrincipalPhoto = jntuhRegisteredFaculty.Photo,
                        PrincipaldateOfreliving = jntuhCollegeFacultytracking != null && jntuhCollegeFacultytracking.relevingdate != null ? jntuhCollegeFacultytracking.relevingdate.Value.ToString("dd-MM-yyyy") : "",
                        dateOfAppointment = jntuhCollegeFacultytracking != null && jntuhCollegeFacultytracking.FacultyJoinDate != null ? jntuhCollegeFacultytracking.FacultyJoinDate.Value.ToString("dd-MM-yyyy") : "",
                    };
                    principalDirector.Add(objprindetails);
                }
            }
            ViewBag.prinipalReglst = principalDirector;
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AddPrincipal()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            //var facultyId = 0;
            TempData["path"] = null;
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser != null)
            {
                var userId = Convert.ToInt32(membershipUser.ProviderUserKey);

                var userCollegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
                if (userCollegeId == 375)
                {
                    userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                ViewBag.collegeId = Utilities.EncryptString(userCollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            }
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(i => i.actualYear > 2013).Select(s => s).OrderByDescending(i => i.id).ToList();
            //Colleges and Designation Drop Dow Codding written by Narayana Reddy on 11-02-2020
            var collegesList = db.jntuh_college.Where(c => c.isActive && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeName + " (" + c.collegeCode + ")" }).OrderBy(c => c.CollegeName).ToList();
            var colleges = collegesList.Select(s => new SelectListItem { Value = s.CollegeId.ToString(), Text = s.CollegeName }).ToList();
            colleges.Add(new SelectListItem { Value = "375", Text = "Others" });
            ViewBag.Institutions = colleges;
            var designations = db.jntuh_designation.Where(e => e.isActive).Select(a => new { Id = a.id, designation = a.designation }).Take(4).ToList();
            ViewBag.Designation = designations;
            return PartialView("AddPrincipal", new PrincipalDirector());
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddPrincipal(PrincipalDirector princial)
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("LogOn", "Account");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            SavePrincialInformation(princial, userId);
            return RedirectToAction("Index");
        }

        public void SavePrincialInformation(PrincipalDirector item1, int userId)
        {
            const string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS/CollegePrincipalAadhaar";
            const string facultyappointmentletters = "~/Content/Upload/College/Faculty/AppointmentLetters";
            const string facultyrelevingletters = "~/Content/Upload/College/Faculty/RelevingLetters";
            const string facultypreviouscollegescms = "~/Content/Upload/College/Faculty/PreviousSCMs";
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 0)
            {
                userCollegeId = item1.collegeId;
            }
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //var resultMessagePrincipal = string.Empty;
            //var principal = new jntuh_college_principal_registered();
            var objArt = new jntuh_college_facultytracking();
            //var strCollegeCode = db.jntuh_college.Find(userCollegeId).collegeCode;
            var jntuhRegisteredFaculty = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == item1.RegistrationNumber).Select(e => e).FirstOrDefault();
            //var jntuh_registered_faculty_flag = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == Item1.RegistrationNumber && (f.Blacklistfaculy == true || f.AbsentforVerification == true)).Select(e => e).FirstOrDefault();
            //var collegefacultyregistered_CollegeId = db.jntuh_college_faculty_registered.Where(p => p.RegistrationNumber == Item1.RegistrationNumber).Select(e => e.collegeId).SingleOrDefault();
            //var collegePrincipalregistered = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == userCollegeId).Select(p => p).FirstOrDefault();

            if (jntuhRegisteredFaculty == null)
            {
                //resultMessagePrincipal = "NotValid";
                TempData["ERROR"] = "Invalid Registration Number";
            }
            else
            {
                if (item1.PrincipalAadharPhotoDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                    }
                    var ext = Path.GetExtension(item1.PrincipalAadharPhotoDocument.FileName);
                    if (ext != null && (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG")))
                    {
                        if (item1.PrincipalAadharDocument == null)
                        {
                            var fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuhRegisteredFaculty.FirstName.Substring(0, 1) + "-" + jntuhRegisteredFaculty.LastName.Substring(0, 1);
                            item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                                fileName, ext));
                            objArt.aadhaardocument = string.Format("{0}{1}", fileName, ext);
                        }
                        else
                        {
                            var fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuhRegisteredFaculty.FirstName.Substring(0, 1) + "-" + jntuhRegisteredFaculty.LastName.Substring(0, 1);
                            item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(aadhaarCardsPath),
                                fileName, ext));
                            objArt.aadhaardocument = string.Format("{0}{1}", fileName, ext);
                        }
                    }
                }
                //else if (item1.PrincipalAadharDocument != null)
                //{
                //    objArt.aadhaardocument = item1.PrincipalAadharDocument;
                //}

                if (item1.PrincipalAppointmentDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                    }
                    var ext = Path.GetExtension(item1.PrincipalAppointmentDocument.FileName);
                    if (ext != null && ext.ToUpper().Equals(".PDF"))
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        jntuhRegisteredFaculty.FirstName.Substring(0, 1) + "-" + jntuhRegisteredFaculty.LastName.Substring(0, 1);
                        item1.PrincipalAppointmentDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                            fileName, ext));
                        item1.AppointmentDocumentView = string.Format("{0}{1}", fileName, ext);
                        objArt.FacultyJoinDocument = item1.AppointmentDocumentView;
                    }
                }
                if (item1.PrincipalRelivingDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                    }
                    var ext = Path.GetExtension(item1.PrincipalRelivingDocument.FileName);
                    if (ext != null && ext.ToUpper().Equals(".PDF"))
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        jntuhRegisteredFaculty.FirstName.Substring(0, 1) + "-" + jntuhRegisteredFaculty.LastName.Substring(0, 1);
                        item1.PrincipalRelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                            fileName, ext));
                        item1.RelivingDocumentView = string.Format("{0}{1}", fileName, ext);
                        objArt.relevingdocumnt = item1.RelivingDocumentView;
                    }
                }
                //Optional
                if (item1.PrincipalSelectionCommitteeDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                    }
                    var ext = Path.GetExtension(item1.PrincipalSelectionCommitteeDocument.FileName);
                    if (ext != null && ext.ToUpper().Equals(".PDF"))
                    {
                        var fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        jntuhRegisteredFaculty.FirstName.Substring(0, 1) + "-" + jntuhRegisteredFaculty.LastName.Substring(0, 1);
                        item1.PrincipalSelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                            fileName, ext));
                        item1.SelectionCommitteeDocumentView = string.Format("{0}{1}", fileName, ext);
                        objArt.scmdocument = item1.SelectionCommitteeDocumentView;
                    }
                }
                try
                {
                    objArt.academicYearId = item1.AcademicYearId;
                    objArt.previousworkingcollegeid = item1.Previouscollegeid;
                    objArt.RegistrationNumber = item1.RegistrationNumber;
                    objArt.FacultyJoinDate = Utilities.DDMMYY2MMDDYY(item1.dateOfAppointment);
                    objArt.aadhaarnumber = item1.PrincipalAadhaarNumber;
                    objArt.collegeId = userCollegeId;
                    objArt.designation = null;
                    objArt.DepartmentId = 0;
                    objArt.SpecializationId = 0;
                    objArt.ActionType = 1;
                    objArt.FacultyType = "Principal";
                    objArt.FacultyStatus = "S";
                    objArt.Reasion = "Principal Insert";
                    objArt.relevingdate = Utilities.DDMMYY2MMDDYY(item1.PrincipaldateOfResignation);
                    //objArt.FacultyJoinDate = DateTime.Now;
                    objArt.payscale = item1.PrincipalSalary;
                    objArt.Createdon = DateTime.Now;
                    objArt.CreatedBy = userId;
                    objArt.Updatedon = null;
                    objArt.UpdatedBy = null;
                    objArt.isActive = true;
                    db.jntuh_college_facultytracking.Add(objArt);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    TempData["ERROR"] = e.Message;
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeletePrincipal(string id)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var userId = Convert.ToInt32(userdata.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (!string.IsNullOrEmpty(id))
            {
                var facultytracking = db.jntuh_college_facultytracking.Find(Convert.ToInt32(id));
                db.jntuh_college_facultytracking.Remove(facultytracking);
                db.SaveChanges();
                //if (true)
                //{
                //    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                //    objART.academicYearId = facultytracking.academicYearId;
                //    objART.collegeId = facultytracking.collegeId;
                //    objART.RegistrationNumber = facultytracking.RegistrationNumber;
                //    objART.DepartmentId = 0;
                //    objART.SpecializationId = 0;
                //    objART.ActionType = 2;
                //    objART.FacultyType = "Principal";
                //    objART.FacultyStatus = "Y";
                //    objART.Reasion = "Principal Deleted by College Successfully.";
                //    objART.FacultyJoinDate = Principal.createdOn;
                //    objART.Createdon = DateTime.Now;
                //    objART.CreatedBy = userId;
                //    objART.Updatedon = null;
                //    objART.UpdatedBy = null;
                //    db.jntuh_college_facultytracking.Add(objART);
                //    db.SaveChanges();
                //    TempData["SUCCESS"] = "Record Deleted Successfully";
                //}
            }
            else
            {
                TempData["ERROR"] = "Principal Not Found.Please try again..";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}        
            //return RedirectToAction("College", "Dashboard");
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }

            int userID = Convert.ToInt32(userdata.ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID <= 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PPD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            PrincipalDirector principal = new PrincipalDirector();
            CollgeDirector director = new CollgeDirector();

            var specs = new List<DistinctSpecializations>();
            var depts = new List<DistinctDepartments>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();

            jntuh_college_principal_registered _principal = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == userCollegeID).Select(p => p).FirstOrDefault();

            if (_principal != null)
            {
                jntuh_registered_faculty prinicipaldetails = db.jntuh_registered_faculty.Where(p => p.RegistrationNumber == _principal.RegistrationNumber).Select(p => p).FirstOrDefault();
                if (prinicipaldetails != null)
                {
                    principal.id = _principal.id;
                    principal.PrincipalId = prinicipaldetails.id;
                    principal.collegeId = _principal.collegeId;
                    principal.RegistrationNumber = _principal.RegistrationNumber;
                    principal.firstName = prinicipaldetails.FirstName;
                    principal.lastName = prinicipaldetails.MiddleName;
                    principal.surname = prinicipaldetails.LastName;
                    principal.departmentName = db.jntuh_department.Where(d => d.id == prinicipaldetails.DepartmentId).Select(d => d.departmentName).SingleOrDefault();
                    principal.dateOfAppointment = prinicipaldetails.DateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfAppointment.ToString()).ToString();
                    principal.dateOfBirth = prinicipaldetails.DateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfBirth.ToString()).ToString();
                    principal.fax = db.jntuh_address.Where(c => c.collegeId == userCollegeID).Select(c => c.fax).FirstOrDefault();
                    principal.landline = db.jntuh_address.Where(c => c.collegeId == userCollegeID).Select(c => c.landline).FirstOrDefault();
                    principal.mobile = prinicipaldetails.Mobile;
                    principal.email = prinicipaldetails.Email;
                    principal.PrincipalPhoto = prinicipaldetails.Photo;
                }
            }

            jntuh_college_principal_director _director = db.jntuh_college_principal_director.Where(a => a.collegeId == userCollegeID && a.type == "DIRECTOR").Select(d => d).FirstOrDefault();
            if (_director != null)
            {
                director.id = _director.id;
                director.firstName = _director.firstName;
                director.lastName = _director.lastName;
                director.surname = _director.surname;
                director.qualificationId = _director.qualificationId;
                //director.qualification = db.jntuh_qualification.Where(s => s.id == _director.qualificationId).Select(a => a.qualification).FirstOrDefault();
                director.dateOfAppointment = _director.dateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfAppointment.ToString()).ToString();
                director.dateOfBirth = _director.dateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfBirth.ToString()).ToString();
                director.fax = _director.fax;
                director.landline = _director.landline;
                director.mobile = _director.mobile;
                director.email = _director.email;
                director.DirectorPhoto = _director.photo;
                director.phdId = _director.phdId;
                director.phd = db.jntuh_phd_subject.Where(q => q.id == _director.phdId).Select(a => a.phdSubjectName).FirstOrDefault();
                director.phdFromUniversity = _director.phdFromUniversity;
                director.phdYear = Convert.ToInt32(_director.phdYear);
                director.departmentId = _director.departmentId;
                director.department = departments.Where(d => d.id == _director.departmentId).Select(z => z.departmentName).FirstOrDefault();
            }

            List<int> collegespecs = new List<int>();

            collegespecs.AddRange(db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).Select(i => i.specializationId).Distinct().ToArray());

            int[] degreeIds = (from a in specializations
                               join b in departments on a.departmentId equals b.id
                               join c in degrees on b.degreeId equals c.id
                               where collegespecs.Contains(a.id)
                               select c.id).Distinct().ToArray();

            if (degreeIds.Contains(4))
            {
                var humanitesSpeci = new[] { 37, 48, 42, 31, 154 };
                collegespecs.AddRange(humanitesSpeci);
            }

            if (collegespecs.Contains(154))
            {
                var othersSpeci = new[] { 155, 156, 157, 158 };
                collegespecs.AddRange(othersSpeci);
            }

            collegespecs.Remove(154);
            int[] degreeids = { 4, 3, 5, 6, 7 };
            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);
                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId && degreeids.Contains(i.degreeId));
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = deptment.departmentDescription, DepartmentId = specid.departmentId });
                }
            }
            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);
            return View("View", model: new Tuple<PrincipalDirector, CollgeDirector>(principal, director));
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult PrincipalEdit(string fid, string collegeId)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (!string.IsNullOrEmpty(fid))
            {
                //  facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Changed by Naushad Khan
                facultyId = Convert.ToInt32(fid);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            PrincipalDirector faculty = new PrincipalDirector();
            jntuh_registered_faculty regfaculty = new jntuh_registered_faculty();
            if (facultyId != 0)
            {
                regfaculty = db.jntuh_registered_faculty.Find(facultyId);
                var jntuhprincipal = db.jntuh_college_principal_registered.Where(e => e.RegistrationNumber.Trim() == regfaculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                if (jntuhprincipal == null)
                {
                    TempData["Error"] = "Faculty Data not found.";
                    return RedirectToAction("Teaching", "Faculty");
                }
                faculty.PrincipalId = regfaculty.id;
                faculty.RegistrationNumber = jntuhprincipal.RegistrationNumber.Trim();
                faculty.id = jntuhprincipal.id;
                faculty.collegeId = userCollegeID;
                faculty.firstName = regfaculty.FirstName;
                faculty.lastName = regfaculty.MiddleName;
                faculty.surname = regfaculty.LastName;
                faculty.PrincipalAadhaarNumber = jntuhprincipal.AadhaarNumber;
                faculty.PrincipalAadharDocument = jntuhprincipal.AadhaarDocument;
            }

            ViewBag.facId = facultyId;


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();



            //Colleges and Designation Drop Dow Codding written by Narayana Reddy on 11-02-2020
            List<SelectListItem> colleges = new List<SelectListItem>();
            var colleges_list = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeName + " (" + c.collegeCode + ")" }).OrderBy(c => c.CollegeName).ToList();
            colleges = colleges_list.Select(s => new SelectListItem { Value = s.CollegeId.ToString(), Text = s.CollegeName }).ToList();
            colleges.Add(new SelectListItem { Value = "375", Text = "Others" });
            ViewBag.Institutions = colleges;
            var Designation = db.jntuh_designation.Where(e => e.isActive == true).Select(a => new { Id = a.id, designation = a.designation }).Take(4).ToList();
            ViewBag.Designation = Designation;


            jntuh_registered_faculty_experience facultyexperiance =
                db.jntuh_registered_faculty_experience.Where(
                    r => r.createdBycollegeId == userCollegeID && r.facultyId == regfaculty.id && r.OtherDesignation == "Principal")
                    .Select(s => s).ToList()
                    .LastOrDefault();
            if (facultyexperiance != null)
            {
                faculty.ExperienceId = facultyexperiance.Id;
                faculty.PrincipalId = facultyexperiance.facultyId;
                faculty.Previouscollegeid = (int)facultyexperiance.collegeId;
                faculty.Otherscollegename = facultyexperiance.OtherCollege;
                if (facultyexperiance.facultyDateOfAppointment != null)
                {
                    DateTime date = Convert.ToDateTime(facultyexperiance.facultyDateOfAppointment);
                    faculty.dateOfAppointment = date.ToString("dd/MM/yyyy").Split(' ')[0];
                }
                if (facultyexperiance.facultyDateOfResignation != null)
                {
                    DateTime date = Convert.ToDateTime(facultyexperiance.facultyDateOfResignation);
                    faculty.PrincipaldateOfResignation = date.ToString("dd/MM/yyyy").Split(' ')[0];
                }
                faculty.PrincipalSalary = facultyexperiance.facultySalary;
                //faculty.dateOfAppointment = facultyexperiance.facultyDateOfAppointment.ToString();
                //faculty.dateOfResignation = facultyexperiance.facultyDateOfResignation.ToString();
                faculty.AppointmentDocumentView = facultyexperiance.facultyJoiningOrder;
                faculty.RelivingDocumentView = facultyexperiance.facultyRelievingLetter;
                faculty.SelectionCommitteeDocumentView = facultyexperiance.FacultySCMDocument;
                //if (faculty.ViewRelivingDocument == null)
                //{
                //    faculty.Principalfresherexperiance = "Fresher";
                //}
                //else
                //{
                //    faculty.Principalfresherexperiance = "Experienced";
                //}               

            }
            return PartialView("PrincipalEdit", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult PrincipalEdit(PrincipalDirector princial)
        {
            SavePrincialInformation(princial);
            return View("View");
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }

            int userID = Convert.ToInt32(userdata.ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (collegeId != null)
                {
                    if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_PrincipalDirector");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PPD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_PrincipalDirector");
                }
            }

            List<SelectListItem> MemberType = new List<SelectListItem>();
            MemberType.Add(new SelectListItem() { Text = "Principal", Value = "1" });
            MemberType.Add(new SelectListItem() { Text = "Director", Value = "2" });
            ViewBag.MemberType = MemberType.Select(a => new
            {
                typeId = a.Value,
                type = a.Text
            });

            var specs = new List<DistinctSpecializations>();
            var depts = new List<DistinctDepartments>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            PrincipalDirector principal = new PrincipalDirector();
            jntuh_college_principal_registered _principal = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == userCollegeID).Select(p => p).FirstOrDefault();

            if (_principal != null)
            {
                jntuh_registered_faculty prinicipaldetails = db.jntuh_registered_faculty.Where(p => p.RegistrationNumber == _principal.RegistrationNumber).Select(p => p).SingleOrDefault();
                principal.id = _principal.id;
                principal.collegeId = userCollegeID;
                principal.RegistrationNumber = _principal.RegistrationNumber;
                principal.firstName = prinicipaldetails.FirstName;
                principal.lastName = prinicipaldetails.MiddleName;
                principal.surname = prinicipaldetails.LastName;
                principal.PrincipalAadhaarNumber = _principal.AadhaarNumber;
                principal.PrincipalAadharDocument = _principal.AadhaarDocument;
                principal.departmentName = db.jntuh_department.Where(d => d.id == prinicipaldetails.DepartmentId).Select(d => d.departmentName).SingleOrDefault();
                principal.dateOfAppointment = prinicipaldetails.DateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfAppointment.ToString()).ToString();

                principal.dateOfBirth = prinicipaldetails.DateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfBirth.ToString()).ToString();
                principal.fax = db.jntuh_address.Where(c => c.collegeId == userCollegeID).Select(c => c.fax).FirstOrDefault();
                principal.landline = db.jntuh_address.Where(c => c.collegeId == userCollegeID).Select(c => c.landline).FirstOrDefault();
                principal.mobile = prinicipaldetails.Mobile;
                principal.email = prinicipaldetails.Email;
                principal.PrincipalPhoto = prinicipaldetails.Photo;
                principal.createdOn = _principal.createdOn;
                principal.createdBy = _principal.createdBy;
            }

            CollgeDirector director = new CollgeDirector();
            jntuh_college_principal_director _director = db.jntuh_college_principal_director.Where(a => a.collegeId == userCollegeID && a.type == "DIRECTOR").Select(d => d).FirstOrDefault();
            if (_director != null)
            {
                director.id = _director.id;
                director.collegeId = userCollegeID;
                director.firstName = _director.firstName;
                director.lastName = _director.lastName;
                director.surname = _director.surname;
                director.qualificationId = _director.qualificationId;
                director.dateOfAppointment = _director.dateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfAppointment.ToString()).ToString();
                director.dateOfBirth = _director.dateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfBirth.ToString()).ToString();
                director.fax = _director.fax;
                director.landline = _director.landline;
                director.mobile = _director.mobile;
                director.email = _director.email;
                director.DirectorPhoto = _director.photo;
                director.phdId = _director.phdId;
                director.phd = db.jntuh_phd_subject.Where(q => q.id == _director.phdId).Select(a => a.phdSubjectName).FirstOrDefault();
                director.phdFromUniversity = _director.phdFromUniversity;
                director.phdYear = Convert.ToInt32(_director.phdYear);
                director.departmentId = _director.departmentId;
                director.department = departments.Where(d => d.id == _director.departmentId).Select(z => z.departmentName).FirstOrDefault();
                //TempData["DirectorEdit"] = true;
            }

            List<int> collegespecs = new List<int>();

            collegespecs.AddRange(db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).Select(i => i.specializationId).Distinct().ToArray());

            int[] degreeIds = (from a in specializations
                               join b in departments on a.departmentId equals b.id
                               join c in degrees on b.degreeId equals c.id
                               where collegespecs.Contains(a.id)
                               select c.id).Distinct().ToArray();

            if (degreeIds.Contains(4))
            {
                var humanitesSpeci = new[] { 37, 48, 42, 31, 154 };
                collegespecs.AddRange(humanitesSpeci);
            }

            if (collegespecs.Contains(154))
            {
                var othersSpeci = new[] { 155, 156, 157, 158 };
                collegespecs.AddRange(othersSpeci);
            }

            collegespecs.Remove(154);
            int[] degreeids = { 4, 3, 5, 6, 7 };
            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);
                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId && degreeids.Contains(i.degreeId));
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        specs.Add(new DistinctSpecializations { DepartmentId = specid.departmentId, DepartmentName = deptment.departmentDescription, SpecializationId = specid.id });
                }
            }
            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);
            ViewBag.phd = db.jntuh_phd_subject.Where(s => s.isActive == true).ToList();
            return View("Edit", model: new Tuple<PrincipalDirector, CollgeDirector>(principal, director));
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(PrincipalDirector Item1, CollgeDirector Item2, int? typeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Item1.collegeId;
                if (userCollegeID == 0)
                {
                    userCollegeID = Item2.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            Item1.collegeId = userCollegeID;
            Item2.collegeId = userCollegeID;
            Item2.phdYear = Item2.phdYear == null ? 0 : (int)Item2.phdYear;
            //if (typeId != null && typeId != 0)
            //{
            //    if (typeId == 1)
            //        SavePrincialInformation(Item1);
            //    else if (typeId == 2)
            //        
            //}
            SaveDirectorInformation(Item2);
            return RedirectToAction("View");
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult Delete(string fid, string collegeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (!string.IsNullOrEmpty(fid))
            {
                //  facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Changed by Naushad Khan
                facultyId = Convert.ToInt32(fid);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            PrincipalDirector faculty = new PrincipalDirector();
            jntuh_registered_faculty regfaculty = new jntuh_registered_faculty();
            if (facultyId != 0)
            {
                regfaculty = db.jntuh_registered_faculty.Find(facultyId);
                var jntuhprincipal = db.jntuh_college_principal_registered.Where(e => e.RegistrationNumber.Trim() == regfaculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                if (jntuhprincipal == null)
                {
                    TempData["Error"] = "Faculty Data not found.";
                    return RedirectToAction("Teaching", "Faculty");
                }
                faculty.PrincipalId = regfaculty.id;
                faculty.RegistrationNumber = jntuhprincipal.RegistrationNumber.Trim();
                faculty.id = jntuhprincipal.id;
                faculty.PrincipalId = regfaculty.id;
                faculty.collegeId = userCollegeID;
                faculty.firstName = regfaculty.FirstName;
                faculty.lastName = regfaculty.MiddleName;
                faculty.surname = regfaculty.LastName;
                faculty.PrincipalAadhaarNumber = jntuhprincipal.AadhaarNumber;
                faculty.PrincipalAadharDocument = jntuhprincipal.AadhaarDocument;
            }

            ViewBag.facId = facultyId;


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            return PartialView("Delete", faculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult Delete(PrincipalDirector principal)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(userdata.ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int presentAyId = db.jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (principal.PrincipalId != 0)
            {
                var Principal = db.jntuh_college_principal_registered.Where(a => a.collegeId == userCollegeID).Select(a => a).FirstOrDefault();
                if (Principal != null)
                {
                    var regfacultydata = db.jntuh_registered_faculty.Find(principal.PrincipalId);
                    jntuh_registered_faculty_experience facultyexperiance = new jntuh_registered_faculty_experience();
                    //
                    string facultyappointmentletters = "~/Content/Upload/College/Faculty/AppointmentLetters";
                    string facultyrelevingletters = "~/Content/Upload/College/Faculty/RelevingLetters";
                    string facultypreviouscollegescms = "~/Content/Upload/College/Faculty/PreviousSCMs";

                    facultyexperiance.facultyId = principal.PrincipalId;
                    facultyexperiance.createdBycollegeId = userCollegeID;


                    if (principal.PrincipalRelivingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                        }

                        var ext = Path.GetExtension(principal.PrincipalRelivingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            regfacultydata.FirstName.Substring(0, 1) + "-" + regfacultydata.LastName.Substring(0, 1);
                            principal.PrincipalRelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                                fileName, ext));
                            principal.RelivingDocumentView = string.Format("{0}{1}", fileName, ext);
                            facultyexperiance.facultyRelievingLetter = principal.RelivingDocumentView;
                        }
                    }

                    ////Optional
                    //if (faculty.SelectionCommitteeDocument != null)
                    //{
                    //    if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                    //    {
                    //        Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                    //    }

                    //    var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);

                    //    if (ext.ToUpper().Equals(".PDF"))
                    //    {
                    //        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                    //        regfacultydata.FirstName.Substring(0, 1) + "-" + regfacultydata.LastName.Substring(0, 1);
                    //        faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                    //            fileName, ext));
                    //        faculty.ViewSelectionCommitteeDocument = string.Format("{0}{1}", fileName, ext);
                    //        facultyexperiance.FacultySCMDocument = faculty.ViewSelectionCommitteeDocument;
                    //    }
                    //}
                    //if (faculty.Previouscollegeid == 0)
                    //{
                    //    facultyexperiance.collegeId = userCollegeID;
                    //}
                    //else
                    //{
                    //    if (faculty.Previouscollegeid == 375)
                    //    {
                    //        facultyexperiance.collegeId = faculty.Previouscollegeid;
                    //        facultyexperiance.OtherCollege = faculty.Otherscollegename;
                    //    }
                    //    else
                    //    {
                    //        facultyexperiance.collegeId = faculty.Previouscollegeid;
                    //    }
                    //}

                    facultyexperiance.collegeId = userCollegeID;
                    facultyexperiance.facultyDesignationId = 4;
                    facultyexperiance.OtherDesignation = "Principal";

                    if (!String.IsNullOrEmpty(principal.dateOfAppointment))
                        facultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(principal.dateOfAppointment);
                    if (!String.IsNullOrEmpty(principal.PrincipaldateOfResignation))
                        facultyexperiance.facultyDateOfResignation =
                        Models.Utilities.DDMMYY2MMDDYY(principal.PrincipaldateOfResignation);

                    facultyexperiance.createdBy = userID;
                    facultyexperiance.createdOn = DateTime.Now;
                    facultyexperiance.updatedBy = null;
                    facultyexperiance.updatedOn = null;
                    db.jntuh_registered_faculty_experience.Add(facultyexperiance);
                    db.SaveChanges();


                    db.jntuh_college_principal_registered.Remove(Principal);
                    db.SaveChanges();

                    var jntuhPrinicalTrackingData =
                        db.jntuh_college_facultytracking.AsNoTracking()
                            .Where(
                                i =>
                                    i.RegistrationNumber == Principal.RegistrationNumber &&
                                    i.collegeId == Principal.collegeId && i.isActive == true && i.ActionType != 2 && i.FacultyType == "Principal")
                            .Select(e => e)
                            .OrderByDescending(i => i.Id)
                            .FirstOrDefault();

                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    //jntuh_attendence_registrationnumberstracking objART = new jntuh_attendence_registrationnumberstracking();
                    objART.academicYearId = presentAyId;
                    objART.collegeId = Principal.collegeId;
                    objART.RegistrationNumber = Principal.RegistrationNumber;
                    objART.DepartmentId = 0;
                    objART.SpecializationId = 0;
                    objART.ActionType = 2;
                    objART.FacultyType = "Principal";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Principal Deleted by College Successfully.";
                    // objART.FacultyStatus = "Y";
                    //  objART.Reasion = "Faculty Deleted by College Successfully.";
                    objART.FacultyJoinDate = Principal.createdOn;
                    if (jntuhPrinicalTrackingData != null)
                    {
                        objART.previousworkingcollegeid = jntuhPrinicalTrackingData.previousworkingcollegeid;
                        objART.scmdocument = jntuhPrinicalTrackingData.scmdocument;
                        objART.FacultyJoinDate = jntuhPrinicalTrackingData.FacultyJoinDate;
                        objART.FacultyJoinDocument = jntuhPrinicalTrackingData.FacultyJoinDocument;
                        objART.aadhaarnumber = jntuhPrinicalTrackingData.aadhaarnumber;
                        objART.aadhaardocument = jntuhPrinicalTrackingData.aadhaardocument;
                        objART.payscale = jntuhPrinicalTrackingData.payscale;
                        objART.designation = jntuhPrinicalTrackingData.designation;
                    }
                    objART.relevingdate = facultyexperiance.facultyDateOfResignation;
                    objART.relevingdocumnt = facultyexperiance.facultyRelievingLetter;
                    objART.isActive = true;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Principal is Deleted Successfully.";
                }
            }

            return RedirectToAction("View");
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult DeleteDirector()
        {
            //return RedirectToAction("Index", "UnderConstruction");
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(userdata.ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID != 0)
            {
                var college_Director =
                    db.jntuh_college_principal_director.Where(r => r.collegeId == userCollegeID)
                        .Select(s => s)
                        .FirstOrDefault();
                if (college_Director != null)
                {
                    db.jntuh_college_principal_director.Remove(college_Director);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Principal is Deleted Successfully.";

                }
                else
                {
                    TempData["ERROR"] = "No Data found";
                }
            }

            return RedirectToAction("View");
        }

        private void SavePrincialInformation(PrincipalDirector Item1)
        {
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS/CollegePrincipalAadhaar";
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Item1.collegeId;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            bool Principal = false;
            var resultMessagePrincipal = string.Empty;

            jntuh_college_principal_registered principal = new jntuh_college_principal_registered();

            string strCollegeCode = db.jntuh_college.Find(userCollegeID).collegeCode;

            jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == Item1.RegistrationNumber).Select(e => e).FirstOrDefault();
            jntuh_registered_faculty jntuh_registered_faculty_flag = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == Item1.RegistrationNumber && (f.Blacklistfaculy == true || f.AbsentforVerification == true)).Select(e => e).FirstOrDefault();
            var collegefacultyregistered_CollegeId = db.jntuh_college_faculty_registered.Where(p => p.RegistrationNumber == Item1.RegistrationNumber).Select(e => e.collegeId).SingleOrDefault();
            jntuh_college_principal_registered collegePrincipalregistered = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == userCollegeID).Select(p => p).FirstOrDefault();

            if (jntuh_registered_faculty == null)
            {
                resultMessagePrincipal = "NotValid";
                TempData["ERROR"] = "Invalid Registration Number";
            }
            else if (jntuh_registered_faculty_flag != null)
            {
                if (jntuh_registered_faculty_flag.Blacklistfaculy == true && jntuh_registered_faculty_flag.AbsentforVerification == true)
                {
                    TempData["ERROR"] = "Faculty is Blacklisted due to possessing of ingenuine UG/PG/Ph.D. Certificates. And Faculty is made inactive due to your absence for physical verification.";
                }
                else
                {
                    if (jntuh_registered_faculty_flag.Blacklistfaculy == true)
                        TempData["ERROR"] = "Faculty is Blacklisted due to possessing of ingenuine UG/PG/Ph.D. Certificates.";
                    else if (jntuh_registered_faculty_flag.AbsentforVerification == true)
                        TempData["ERROR"] = "Faculty is made inactive due to your absence for physical verification.";
                }
            }
            else
            {
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var actualYear = jntuh_academic_year.Where(q => q.isPresentAcademicYear == true && q.isActive == true).Select(a => a.actualYear).FirstOrDefault();
                var academicYearId = jntuh_academic_year.Where(q => q.actualYear == (actualYear + 1)).Select(x => x.id).FirstOrDefault();

                if (collegefacultyregistered_CollegeId != null && collegefacultyregistered_CollegeId != 0)
                {
                    if (collegefacultyregistered_CollegeId == userCollegeID)
                    {
                        if (collegePrincipalregistered == null || collegePrincipalregistered.collegeId == 0)
                        {
                            if (db.jntuh_college_principal_registered.Where(p => p.RegistrationNumber == Item1.RegistrationNumber).Count() != 0)
                            {
                                TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                            }
                            else
                            {
                                JsonResult AadharNumberCheck = CheckAadharNumber(Item1.PrincipalAadhaarNumber, Item1.RegistrationNumber);
                                string status = AadharNumberCheck.Data.ToString();
                                if (status == "True")
                                {
                                    principal.collegeId = userCollegeID;
                                    principal.RegistrationNumber = Item1.RegistrationNumber;
                                    principal.AadhaarNumber = Item1.PrincipalAadhaarNumber;
                                    if (Item1.PrincipalAadharPhotoDocument != null)
                                    {
                                        if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                                        {
                                            Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                                        }

                                        var ext = Path.GetExtension(Item1.PrincipalAadharPhotoDocument.FileName);
                                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                        {
                                            if (Item1.PrincipalAadharDocument == null)
                                            {
                                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                                Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                                                    fileName, ext));
                                                Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                                principal.AadhaarDocument = Item1.AadharDocumentView;
                                            }
                                            else
                                            {
                                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                                Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                                                    Server.MapPath(aadhaarCardsPath),
                                                    fileName, ext));
                                                Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                                principal.AadhaarDocument = Item1.AadharDocumentView;
                                            }
                                        }
                                    }
                                    else if (Item1.PrincipalAadharDocument != null)
                                    {
                                        principal.AadhaarDocument = Item1.AadharDocumentView = Item1.PrincipalAadharDocument;
                                    }
                                    principal.createdBy = userID;
                                    principal.createdOn = DateTime.Now;
                                    db.jntuh_college_principal_registered.Add(principal);
                                    resultMessagePrincipal = "Save";
                                    db.SaveChanges();
                                    TempData["Success"] = "Principal Details are Added successfully.";

                                    //jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                                    //objART.academicYearId = academicYearId;
                                    //objART.collegeId = userCollegeID;
                                    //objART.RegistrationNumber = principal.RegistrationNumber;
                                    //objART.DepartmentId = 0;
                                    //objART.SpecializationId = 0;
                                    //objART.ActionType = 1;
                                    //objART.FacultyType = "Principal";
                                    //objART.FacultyStatus = "Y";
                                    //objART.Reasion = "Principal Insert";
                                    //objART.FacultyJoinDate = DateTime.Now;
                                    //objART.Createdon = DateTime.Now;
                                    //objART.CreatedBy = userID;
                                    //objART.Updatedon = null;
                                    //objART.UpdatedBy = null;
                                    //db.jntuh_college_facultytracking.Add(objART);
                                    //db.SaveChanges();
                                }
                                else
                                {
                                    TempData["ERROR"] = "Aadhaar Number is already Exists with another Faculty";
                                }
                            }
                        }
                        else if (collegePrincipalregistered.collegeId == Item1.collegeId)
                        {
                            if (db.jntuh_college_principal_registered.Where(p => p.collegeId != userCollegeID && p.RegistrationNumber == Item1.RegistrationNumber).Count() != 0)
                            {
                                TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                            }
                            else
                            {
                                JsonResult AadharNumberCheck = CheckAadharNumber(Item1.PrincipalAadhaarNumber, Item1.RegistrationNumber);
                                string status = AadharNumberCheck.Data.ToString();
                                if (status == "True")
                                {
                                    principal.id = Item1.id;
                                    principal.collegeId = userCollegeID;
                                    principal.RegistrationNumber = Item1.RegistrationNumber;
                                    principal.AadhaarNumber = Item1.PrincipalAadhaarNumber;
                                    if (Item1.PrincipalAadharPhotoDocument != null)
                                    {
                                        if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                                        {
                                            Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                                        }

                                        var ext = Path.GetExtension(Item1.PrincipalAadharPhotoDocument.FileName);
                                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                        {
                                            if (Item1.PrincipalAadharDocument == null)
                                            {
                                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                                Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath), fileName, ext));
                                                Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                                principal.AadhaarDocument = Item1.AadharDocumentView;
                                            }
                                            else
                                            {
                                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                                Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath), fileName, ext));
                                                Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                                principal.AadhaarDocument = Item1.AadharDocumentView;
                                            }
                                        }
                                    }
                                    else if (Item1.PrincipalAadharDocument != null)
                                    {
                                        principal.AadhaarDocument = Item1.AadharDocumentView = Item1.PrincipalAadharDocument;
                                    }

                                    if (collegePrincipalregistered.RegistrationNumber == Item1.RegistrationNumber)
                                    {
                                        principal.createdBy = Item1.createdBy;
                                        principal.createdOn = Item1.createdOn;
                                        principal.updatedBy = userID;
                                        principal.updatedOn = DateTime.Now;
                                    }
                                    else
                                    {
                                        principal.createdBy = userID;
                                        principal.createdOn = DateTime.Now;
                                    }

                                    db.Entry(principal).State = EntityState.Modified;
                                    resultMessagePrincipal = "Update";
                                    db.SaveChanges();
                                    TempData["Success"] = "Principal Details are Updated successfully.";

                                    //jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                                    //objART.academicYearId = academicYearId;
                                    //objART.collegeId = userCollegeID;
                                    //objART.RegistrationNumber = principal.RegistrationNumber.Trim().ToString();
                                    //objART.DepartmentId = 0;
                                    //objART.SpecializationId = 0;
                                    //objART.ActionType = 3;
                                    //objART.FacultyType = "Principal";
                                    //objART.FacultyStatus = "Y";
                                    //objART.Reasion = "Principal Updated";
                                    //objART.FacultyJoinDate = principal.createdOn;
                                    //objART.Createdon = DateTime.Now;
                                    //objART.CreatedBy = userID;
                                    //objART.Updatedon = null;
                                    //objART.UpdatedBy = null;
                                    //db.jntuh_college_facultytracking.Add(objART);
                                    //db.SaveChanges();
                                }
                                else
                                {
                                    TempData["ERROR"] = "Aadhaar Number is already Exists with another Faculty";
                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["ERROR"] = "Faculty is already working in other JNTUH affiliated college.";
                    }
                }
                else
                {
                    if (collegePrincipalregistered == null || collegePrincipalregistered.collegeId == 0)
                    {
                        if (db.jntuh_college_principal_registered.Where(p => p.RegistrationNumber == Item1.RegistrationNumber).Count() != 0)
                        {
                            TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                        }
                        else
                        {
                            JsonResult AadharNumberCheck = CheckAadharNumber(Item1.PrincipalAadhaarNumber, Item1.RegistrationNumber);
                            string status = AadharNumberCheck.Data.ToString();
                            if (status == "True")
                            {
                                principal.collegeId = userCollegeID;
                                principal.RegistrationNumber = Item1.RegistrationNumber;
                                principal.AadhaarNumber = Item1.PrincipalAadhaarNumber;
                                if (Item1.PrincipalAadharPhotoDocument != null)
                                {
                                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                                    {
                                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                                    }

                                    var ext = Path.GetExtension(Item1.PrincipalAadharPhotoDocument.FileName);
                                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                    {
                                        if (Item1.PrincipalAadharDocument == null)
                                        {
                                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                            Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                                                fileName, ext));
                                            Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                            principal.AadhaarDocument = Item1.AadharDocumentView;
                                        }
                                        else
                                        {
                                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                            Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                                                Server.MapPath(aadhaarCardsPath),
                                                fileName, ext));
                                            Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                            principal.AadhaarDocument = Item1.AadharDocumentView;
                                        }
                                    }
                                }
                                else if (Item1.PrincipalAadharDocument != null)
                                {
                                    principal.AadhaarDocument = Item1.AadharDocumentView = Item1.PrincipalAadharDocument;
                                }
                                principal.createdBy = userID;
                                principal.createdOn = DateTime.Now;
                                db.jntuh_college_principal_registered.Add(principal);
                                resultMessagePrincipal = "Save";
                                db.SaveChanges();
                                TempData["Success"] = "Principal Details are Added successfully.";

                                //jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                                //objART.academicYearId = academicYearId;
                                //objART.collegeId = userCollegeID;
                                //objART.RegistrationNumber = principal.RegistrationNumber;
                                //objART.DepartmentId = 0;
                                //objART.SpecializationId = 0;
                                //objART.ActionType = 1;
                                //objART.FacultyType = "Principal";
                                //objART.FacultyStatus = "Y";
                                //objART.Reasion = "Principal Insert";
                                //objART.FacultyJoinDate = DateTime.Now;
                                //objART.Createdon = DateTime.Now;
                                //objART.CreatedBy = userID;
                                //objART.Updatedon = null;
                                //objART.UpdatedBy = null;
                                //db.jntuh_college_facultytracking.Add(objART);
                                //db.SaveChanges();
                            }
                            else
                            {
                                TempData["ERROR"] = "Aadhaar Number is already Exists with another Faculty";
                            }
                        }
                    }
                    else if (collegePrincipalregistered.collegeId == Item1.collegeId)
                    {
                        if (db.jntuh_college_principal_registered.Where(p => p.collegeId != userCollegeID && p.RegistrationNumber == Item1.RegistrationNumber).Count() != 0)
                        {
                            TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                        }
                        else
                        {
                            JsonResult AadharNumberCheck = CheckAadharNumber(Item1.PrincipalAadhaarNumber, Item1.RegistrationNumber);
                            string status = AadharNumberCheck.Data.ToString();
                            if (status == "True")
                            {
                                principal.id = Item1.id;
                                principal.collegeId = userCollegeID;
                                principal.RegistrationNumber = Item1.RegistrationNumber;
                                principal.AadhaarNumber = Item1.PrincipalAadhaarNumber;
                                if (Item1.PrincipalAadharPhotoDocument != null)
                                {
                                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                                    {
                                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                                    }
                                    var ext = Path.GetExtension(Item1.PrincipalAadharPhotoDocument.FileName);
                                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                    {
                                        if (Item1.PrincipalAadharDocument == null)
                                        {
                                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                            Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath), fileName, ext));
                                            Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                            principal.AadhaarDocument = Item1.AadharDocumentView;
                                        }
                                        else
                                        {
                                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                            Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath), fileName, ext));
                                            Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                            principal.AadhaarDocument = Item1.AadharDocumentView;
                                        }
                                    }
                                }
                                else if (Item1.PrincipalAadharDocument != null)
                                {
                                    principal.AadhaarDocument = Item1.AadharDocumentView = Item1.PrincipalAadharDocument;
                                }

                                if (collegePrincipalregistered.RegistrationNumber == Item1.RegistrationNumber)
                                {
                                    principal.createdBy = Item1.createdBy;
                                    principal.createdOn = Item1.createdOn;
                                    principal.updatedBy = userID;
                                    principal.updatedOn = DateTime.Now;
                                }
                                else
                                {
                                    principal.createdBy = userID;
                                    principal.createdOn = DateTime.Now;
                                }
                                db.Entry(principal).State = EntityState.Modified;
                                resultMessagePrincipal = "Update";
                                db.SaveChanges();
                                TempData["Success"] = "Principal Details are Updated successfully.";

                                //jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                                //objART.academicYearId = academicYearId;
                                //objART.collegeId = userCollegeID;
                                //objART.RegistrationNumber = principal.RegistrationNumber.Trim().ToString();
                                //objART.DepartmentId = 0;
                                //objART.SpecializationId = 0;
                                //objART.ActionType = 3;
                                //objART.FacultyType = "Principal";
                                //objART.FacultyStatus = "Y";
                                //objART.Reasion = "Principal Updated";
                                //objART.FacultyJoinDate = principal.createdOn;
                                //objART.Createdon = DateTime.Now;
                                //objART.CreatedBy = userID;
                                //objART.Updatedon = null;
                                //objART.UpdatedBy = null;
                                //db.jntuh_college_facultytracking.Add(objART);
                                //db.SaveChanges();
                            }
                            else
                            {
                                TempData["ERROR"] = "Aadhaar Number is already Exists with another Faculty";
                            }

                        }
                    }
                    else
                    {
                        TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                    }
                }

                //Principal Experiance Saving on 15-02-2020
                jntuh_registered_faculty_experience updatefacultyexperiance =
                db.jntuh_registered_faculty_experience.Where(e => e.Id == Item1.ExperienceId)
                    .Select(s => s)
                    .FirstOrDefault();
                string facultyappointmentletters = "~/Content/Upload/College/Faculty/AppointmentLetters";
                string facultyrelevingletters = "~/Content/Upload/College/Faculty/RelevingLetters";
                string facultypreviouscollegescms = "~/Content/Upload/College/Faculty/PreviousSCMs";
                if (updatefacultyexperiance != null)
                {
                    if (Item1.PrincipalAppointmentDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalAppointmentDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalAppointmentDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                                fileName, ext));
                            Item1.AppointmentDocumentView = string.Format("{0}{1}", fileName, ext);
                            updatefacultyexperiance.facultyJoiningOrder = Item1.AppointmentDocumentView;
                        }
                    }
                    if (Item1.PrincipalRelivingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalRelivingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalRelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                                fileName, ext));
                            Item1.RelivingDocumentView = string.Format("{0}{1}", fileName, ext);
                            updatefacultyexperiance.facultyRelievingLetter = Item1.RelivingDocumentView;
                        }
                    }
                    //Optional
                    if (Item1.PrincipalSelectionCommitteeDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalSelectionCommitteeDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalSelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                                fileName, ext));
                            Item1.SelectionCommitteeDocumentView = string.Format("{0}{1}", fileName, ext);
                            updatefacultyexperiance.FacultySCMDocument = Item1.SelectionCommitteeDocumentView;
                        }
                    }
                    if (Item1.Previouscollegeid == 375)
                    {
                        updatefacultyexperiance.collegeId = Item1.Previouscollegeid;
                        updatefacultyexperiance.OtherCollege = Item1.Otherscollegename;
                    }
                    else
                    {
                        updatefacultyexperiance.collegeId = Item1.Previouscollegeid;
                        updatefacultyexperiance.OtherCollege = null;
                    }

                    updatefacultyexperiance.facultyDesignationId = 4;
                    updatefacultyexperiance.OtherDesignation = "Principal";
                    if (Item1.PrincipalSalary != null)
                        updatefacultyexperiance.facultySalary = Item1.PrincipalSalary;


                    if (!String.IsNullOrEmpty(Item1.dateOfAppointment))
                        updatefacultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(Item1.dateOfAppointment);
                    if (!String.IsNullOrEmpty(Item1.PrincipaldateOfResignation))
                        updatefacultyexperiance.facultyDateOfResignation =
                        Models.Utilities.DDMMYY2MMDDYY(Item1.PrincipaldateOfResignation);
                    updatefacultyexperiance.updatedBy = userID;
                    updatefacultyexperiance.updatedOn = DateTime.Now;
                    db.Entry(updatefacultyexperiance).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    jntuh_registered_faculty_experience facultyexperiance = new jntuh_registered_faculty_experience();

                    facultyexperiance.facultyId = jntuh_registered_faculty.id;
                    facultyexperiance.createdBycollegeId = userCollegeID;
                    if (Item1.PrincipalAppointmentDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalAppointmentDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalAppointmentDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                                fileName, ext));
                            Item1.AppointmentDocumentView = string.Format("{0}{1}", fileName, ext);
                            facultyexperiance.facultyJoiningOrder = Item1.AppointmentDocumentView;
                        }
                    }

                    if (Item1.PrincipalRelivingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalRelivingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalRelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                                fileName, ext));
                            Item1.RelivingDocumentView = string.Format("{0}{1}", fileName, ext);
                            facultyexperiance.facultyRelievingLetter = Item1.RelivingDocumentView;
                        }
                    }

                    //Optional
                    if (Item1.PrincipalSelectionCommitteeDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalSelectionCommitteeDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalSelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                                fileName, ext));
                            Item1.SelectionCommitteeDocumentView = string.Format("{0}{1}", fileName, ext);
                            facultyexperiance.FacultySCMDocument = Item1.SelectionCommitteeDocumentView;
                        }
                    }

                    if (Item1.Previouscollegeid == 375)
                    {
                        facultyexperiance.collegeId = Item1.Previouscollegeid;
                        facultyexperiance.OtherCollege = Item1.Otherscollegename;
                    }
                    else
                    {
                        facultyexperiance.collegeId = Item1.Previouscollegeid;
                    }
                    facultyexperiance.facultyDesignationId = 4;
                    facultyexperiance.OtherDesignation = "Principal";

                    if (Item1.PrincipalSalary != null)
                        facultyexperiance.facultySalary = Item1.PrincipalSalary;
                    if (!String.IsNullOrEmpty(Item1.dateOfAppointment))
                        facultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(Item1.dateOfAppointment);
                    if (!String.IsNullOrEmpty(Item1.PrincipaldateOfResignation))
                        facultyexperiance.facultyDateOfResignation =
                        Models.Utilities.DDMMYY2MMDDYY(Item1.PrincipaldateOfResignation);

                    facultyexperiance.createdBy = userID;
                    facultyexperiance.createdOn = DateTime.Now;
                    facultyexperiance.updatedBy = null;
                    facultyexperiance.updatedOn = null;
                    db.jntuh_registered_faculty_experience.Add(facultyexperiance);
                    db.SaveChanges();
                }
                if (resultMessagePrincipal == "Save")
                {
                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    objART.academicYearId = academicYearId;
                    objART.collegeId = userCollegeID;
                    objART.RegistrationNumber = principal.RegistrationNumber;
                    objART.DepartmentId = 0;
                    objART.SpecializationId = 0;
                    objART.ActionType = 1;
                    objART.FacultyType = "Principal";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Principal Insert";
                    objART.previousworkingcollegeid = Item1.Previouscollegeid;
                    objART.scmdocument = Item1.SelectionCommitteeDocumentView;
                    objART.FacultyJoinDate = Utilities.DDMMYY2MMDDYY(Item1.dateOfAppointment);
                    objART.FacultyJoinDocument = Item1.AppointmentDocumentView;
                    objART.relevingdate = Utilities.DDMMYY2MMDDYY(Item1.PrincipaldateOfResignation);
                    objART.relevingdocumnt = Item1.RelivingDocumentView;
                    objART.aadhaarnumber = Item1.PrincipalAadhaarNumber.Trim();
                    objART.aadhaardocument = Item1.AadharDocumentView;
                    objART.payscale = Item1.PrincipalSalary;
                    objART.designation = null; // Others
                    objART.isActive = true;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                }
                else if (resultMessagePrincipal == "Update")
                {
                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    objART.academicYearId = academicYearId;
                    objART.collegeId = userCollegeID;
                    objART.RegistrationNumber = principal.RegistrationNumber.Trim();
                    objART.DepartmentId = 0;
                    objART.SpecializationId = 0;
                    objART.ActionType = 3;
                    objART.FacultyType = "Principal";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Principal Updated";
                    objART.previousworkingcollegeid = Item1.Previouscollegeid;
                    objART.scmdocument = Item1.SelectionCommitteeDocumentView;
                    objART.FacultyJoinDate = Utilities.DDMMYY2MMDDYY(Item1.dateOfAppointment);
                    objART.FacultyJoinDocument = Item1.AppointmentDocumentView;
                    objART.relevingdate = Utilities.DDMMYY2MMDDYY(Item1.PrincipaldateOfResignation);
                    objART.relevingdocumnt = Item1.RelivingDocumentView;
                    objART.aadhaarnumber = Item1.PrincipalAadhaarNumber.Trim();
                    objART.aadhaardocument = Item1.AadharDocumentView;
                    objART.payscale = Item1.PrincipalSalary;
                    objART.designation = null; // Others
                    objART.isActive = true;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                }
            }
        }

        private void SaveDirectorInformation(CollgeDirector Item2)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
                userCollegeID = Item2.collegeId;
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var resultMessage = string.Empty;
            string strCollegeCode = db.jntuh_college.Find(userCollegeID).collegeCode;
            jntuh_college_principal_director director = new jntuh_college_principal_director();
            if (Item2.firstName != null && Item2.surname != null)
            {
                director.collegeId = userCollegeID;
                director.type = "DIRECTOR";
                director.firstName = Item2.firstName;
                director.lastName = Item2.lastName == null ? string.Empty : Item2.lastName;
                director.surname = Item2.surname;
                director.qualificationId = Item2.qualificationId;
                director.dateOfAppointment = Utilities.DDMMYY2MMDDYY(Item2.dateOfAppointment);
                director.dateOfResignation = Item2.dateOfResignation;
                director.dateOfBirth = Utilities.DDMMYY2MMDDYY(Item2.dateOfBirth);
                director.fax = Item2.fax;
                director.landline = Item2.landline;
                director.mobile = Item2.mobile;
                director.email = Item2.email;
                director.departmentId = Item2.departmentId;
                director.phdId = Item2.phdId;
                director.phdFromUniversity = Item2.phdFromUniversity;
                director.phdYear = Item2.phdYear;

                string photoPath = "~/Content/Upload/PrincipalDirectorPhotos";
                if (Item2.DirectorPhotoDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(photoPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(photoPath));
                    }

                    var ext = Path.GetExtension(Item2.DirectorPhotoDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = "D-" + strCollegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                        Item2.DirectorPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(photoPath), fileName, ext));
                        director.photo = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else if (Item2.DirectorPhoto != null)
                {
                    director.photo = Item2.DirectorPhoto;
                }

                int directorID = db.jntuh_college_principal_director.Where(p => p.collegeId == userCollegeID && p.type == "DIRECTOR").Select(p => p.id).FirstOrDefault();

                if (directorID == 0)
                {
                    resultMessage = "Save";
                    director.createdBy = userID;
                    director.createdOn = DateTime.Now;
                    db.jntuh_college_principal_director.Add(director);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Director Details are Added successfully.";

                }
                else
                {
                    director.id = directorID;
                    director.createdBy = db.jntuh_college_principal_director.Where(p => p.id == directorID && p.type == "DIRECTOR").Select(p => p.createdBy).FirstOrDefault();
                    director.createdOn = db.jntuh_college_principal_director.Where(p => p.id == directorID && p.type == "DIRECTOR").Select(p => p.createdOn).FirstOrDefault();
                    director.updatedBy = userID;
                    director.updatedOn = DateTime.Now;
                    db.Entry(director).State = EntityState.Modified;
                    db.SaveChanges();
                    resultMessage = "Update";
                    TempData["SUCCESS"] = "Director Details are Updated successfully.";
                }
            }
            else
            {
                TempData["ERROR"] = "Director Details are Missing.Please Try";
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College,DataEntry")]
        public JsonResult CheckRegistrationNumber(string RegistrationNumber)
        {
            var isRegistrationNumber = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber.Trim() == RegistrationNumber.Trim()).FirstOrDefault();
            if (isRegistrationNumber != null)
            {
                if (isRegistrationNumber.Blacklistfaculy == true)
                {
                    return Json("Registration Number is in Blacklist.", JsonRequestBehavior.AllowGet);
                }
                else if (isRegistrationNumber.AbsentforVerification == true)
                {
                    return Json("Registration Number is in Inactive.", JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(true);
            }
            else
                return Json("Invalid Registration Number.", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult CheckAadharNumber(string PrincipalAadhaarNumber, string RegistrationNumber)
        {
            var status = aadharcard.validateVerhoeff(PrincipalAadhaarNumber);
            var jntuh_college_faculty_registered =
                db.jntuh_college_faculty_registered.Where(
                    f => f.AadhaarNumber == PrincipalAadhaarNumber && f.RegistrationNumber != RegistrationNumber)
                    .Select(e => e)
                    .Count();
            var jntuh_college_principal_registered =
              db.jntuh_college_principal_registered.Where(
                  f => f.AadhaarNumber == PrincipalAadhaarNumber && f.RegistrationNumber != RegistrationNumber)
                  .Select(e => e)
                  .Count();

            if (status)
            {
                if (jntuh_college_faculty_registered == 0 && jntuh_college_principal_registered == 0)
                {
                    return Json(true);
                }
                else
                {
                    return Json("AadhaarNumber already Exists", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {

                return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AddNewPrincipal()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            PrincipalDirector faculty = new PrincipalDirector();
            jntuh_registered_faculty regfaculty = new jntuh_registered_faculty();


            ViewBag.facId = facultyId;
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            //Colleges and Designation Drop Dow Codding written by Narayana Reddy on 11-02-2020
            List<SelectListItem> colleges = new List<SelectListItem>();
            var colleges_list = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeName + " (" + c.collegeCode + ")" }).OrderBy(c => c.CollegeName).ToList();
            colleges = colleges_list.Select(s => new SelectListItem { Value = s.CollegeId.ToString(), Text = s.CollegeName }).ToList();
            colleges.Add(new SelectListItem { Value = "375", Text = "Others" });
            ViewBag.Institutions = colleges;
            var Designation = db.jntuh_designation.Where(e => e.isActive == true).Select(a => new { Id = a.id, designation = a.designation }).Take(4).ToList();
            ViewBag.Designation = Designation;

            return PartialView("AddNewPrincipal", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddNewPrincipal(PrincipalDirector princial)
        {
            SavePrincialInformation(princial);
            return View("View");
        }
    }
}
