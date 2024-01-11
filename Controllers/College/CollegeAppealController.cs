using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Security;
using UAAAS.Models;
using System.Web.Configuration;
using System.IO;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using Microsoft.Web.Mvc;
using System.Threading;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeAppealController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private string bpharmacycondition;
        private string pharmdcondition;
        private string pharmadpbcondition;
        private decimal pharmadpbrequiredfaculty;
        private decimal BpharmacyrequiredFaculty;
        private decimal PharmDRequiredFaculty;
        private decimal PharmDPBRequiredFaculty;
        private int TotalcollegeFaculty;
        private int Group1PharmacyFaculty;
        private int Group2PharmacyFaculty;
        private int Group3PharmacyFaculty;
        private int Group4PharmacyFaculty;
        private int Group5PharmacyFaculty;
        private int Group6PharmacyFaculty;
        private int Allgroupscount;
        private string PharmacyandPharmDMeet = "";

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult Appelaprincipal()
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuhcollege = db.jntuh_college.AsNoTracking().ToList();
            var intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            var intakelist = new CollegeFacultyWithIntakeReport();
            #region
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    //return RedirectToAction("CollegeDashboard", "Dashboard");
                    return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            // Principal Details
            var strPrincipalRegno = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf.RegistrationNumber).FirstOrDefault();
            var principal = string.Empty;
            var Reason = string.Empty;
            var prinicpalexists = false;
            //Reg nos related online facultyIds
            var regdata = db.jntuh_registered_faculty.FirstOrDefault(rf => strPrincipalRegno == rf.RegistrationNumber);
            ViewBag.principaldata = regdata;
            if (regdata != null)
            {
                if (!string.IsNullOrEmpty(regdata.DeactivationReason))
                    Reason = regdata.DeactivationReason;
                if (regdata.BAS == "Yes")
                {
                    if (!String.IsNullOrEmpty(Reason))
                        Reason += ",Not Fulfilling Biometric Attendance";
                    else
                        Reason += "Not Fulfilling Biometric Attendance";
                }
                if (Reason != "")
                {
                    Reason = Reason;

                }
                else
                {
                    Reason = "Dr. " + regdata.FirstName.First().ToString().ToUpper() + regdata.FirstName.Substring(1) + " " + regdata.LastName.First().ToString().ToUpper() + regdata.LastName.Substring(1);
                    prinicpalexists = true;
                }

                intakelist.RegistrationNumber = regdata.RegistrationNumber;
            }
            else
            {
                Reason = "NO PRINCIPAL";
            }

            ViewBag.PrincipalRegno = Reason;
            if (prinicpalexists == true)

                ViewBag.PrincipalDeficiency = "NO Principal Deficiency";

            else
                ViewBag.PrincipalDeficiency = "Principal Deficiency";
            intakelist.collegeId = collegeid;
            intakelist.collegeName = jntuhcollege.Where(i => i.id == collegeId).Select(i => i.collegeName).FirstOrDefault();
            intakelist.collegeCode = jntuhcollege.Where(i => i.id == collegeId).Select(i => i.collegeCode).FirstOrDefault();

            intakedetailsList.Add(intakelist);
            #endregion
            #region Principal Appeal Deficiency Status

            //  var jntuhAppealPrincipal = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
            var prinipal = db.jntuh_appeal_principal_registered.Where(i => i.collegeId == collegeId && i.academicYearId == prAy && i.PhysicalPresenceonInspection == null).ToList();

            if (prinipal.Count > 0)
            {
                ViewBag.principaldeficiencystatus = true;

            }
            else
            {
                ViewBag.principaldeficiencystatus = false;
            }
            #endregion
            return View(intakedetailsList);
        }

        //Complicance Principal adding
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult Complicanceprincipal(string collegeId, string fid, int deficencycount, int departmentid, string degree, int specializationid)
        {

            int facultyId = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            if (!string.IsNullOrEmpty(fid))
            {
                facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.degree = degree;
            ViewBag.deficiencycount = deficencycount;
            ViewBag.specializationid = specializationid;
            ViewBag.departmentid = departmentid;
            var faculty = new CollegeFaculty
            {
                facultyDepartmentId = departmentid,
                SpecializationId = specializationid,
                DegreeName = degree,
                Facultydeficencycount = deficencycount,
                collegeId = Convert.ToInt16(collegeId)
            };
            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult Complicanceprincipal(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim()).Select(r => r).FirstOrDefault();
            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim()).Select(r => r).FirstOrDefault();
            var jntuh_appeal_faculty = db.jntuh_appeal_principal_registered.Where(r => r.academicYearId == prAy).ToList();
            var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
            string[] RegistrationNumber = jntuh_appeal_faculty.Select(i => i.RegistrationNumber).ToArray();

            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            if (isRegisteredFaculty == null)
            {
                TempData["Error"] = "Invalid Principal Registration Number.";
                return RedirectToAction("Appelaprincipal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (isExistingFaculty != null)
            {
                if (userCollegeID != isExistingFaculty.collegeId)
                {
                    TempData["Error"] = "Principal is already working in other JNTUH affiliated college.";
                    return RedirectToAction("Appelaprincipal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
            }

            var notificationPath = "~/Content/Upload/OnlineAppealDocuments/Principal/NotificationsReports";
            var selectioncommitteePath = "~/Content/Upload/OnlineAppealDocuments/Principal/SelectionCommitteeReports";
            var appointmentorderPath = "~/Content/Upload/OnlineAppealDocuments/Principal/AppointmentOrders";
            var joiningreportpath = "~/Content/Upload/OnlineAppealDocuments/Principal/JoiningReports";
            var phdundertakingdocpath = "~/Content/Upload/OnlineAppealDocuments/Principal/PhdUndertakingReports";
            var PrinicipalAadhaarDocument = "~/Content/Upload/OnlineAppealDocuments/Principal/AadhaarDocuments";
            var deparment = jntuh_deparment.FirstOrDefault(i => i.id == faculty.facultyDepartmentId);


            if (!RegistrationNumber.Contains(faculty.FacultyRegistrationNumber.Trim()))
            {
                //int FacultyId = db.jntuh_college_faculty_registered.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(F => F.id).FirstOrDefault();
                //jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(i => i).FirstOrDefault();

                jntuh_appeal_principal_registered UpdatedFaculty = new jntuh_appeal_principal_registered();
                UpdatedFaculty.collegeId = isRegisteredFaculty.collegeId != null
                    ? (int)isRegisteredFaculty.collegeId
                    : 0;
                UpdatedFaculty.collegeId = faculty.collegeId;
                UpdatedFaculty.academicYearId = prAy;
                UpdatedFaculty.RegistrationNumber = faculty.FacultyRegistrationNumber.Trim();
                UpdatedFaculty.existingFacultyId = isRegisteredFaculty.id;
                UpdatedFaculty.IdentifiedFor = faculty.facultyRecruitedFor;
                //UpdatedFaculty.DepartmentId = faculty.facultyDepartmentId;
                //UpdatedFaculty.SpecializationId = faculty.SpecializationId;

                //var jntuhDepartment = jntuh_deparment.Where(i => i.id == faculty.facultyDepartmentId).FirstOrDefault();
                //if (jntuhDepartment != null)
                //{
                //    UpdatedFaculty.DegreeId = jntuhDepartment.degreeId;
                //}

                UpdatedFaculty.AadhaarNumber = faculty.facultyAadhaarNumber;

                if (faculty.facultyAadharPhotoDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(PrinicipalAadhaarDocument)))
                    {
                        Directory.CreateDirectory(Server.MapPath(PrinicipalAadhaarDocument));
                    }

                    var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);
                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(PrinicipalAadhaarDocument),
                            fileName, ext));
                        UpdatedFaculty.AadhaarDocument = string.Format("{0}/{1}{2}", PrinicipalAadhaarDocument, fileName, ext);
                    }
                }

                if (faculty.NotificationDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(notificationPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(notificationPath));
                    }

                    var ext = Path.GetExtension(faculty.NotificationDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.NotificationDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(notificationPath),
                            fileName, ext));
                        UpdatedFaculty.NOtificationReport = string.Format("{0}/{1}{2}", notificationPath, fileName, ext);
                    }
                }
                if (faculty.SelectionCommitteeDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(selectioncommitteePath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(selectioncommitteePath));
                    }

                    var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(selectioncommitteePath), fileName, ext));
                        UpdatedFaculty.SelectionCommiteMinutes = string.Format("{0}/{1}{2}", selectioncommitteePath,
                            fileName, ext);
                    }
                }
                if (faculty.AppointmentOrderDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(appointmentorderPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(appointmentorderPath));
                    }

                    var ext = Path.GetExtension(faculty.AppointmentOrderDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.AppointmentOrderDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(appointmentorderPath), fileName, ext));
                        UpdatedFaculty.AppointMentOrder = string.Format("{0}/{1}{2}", appointmentorderPath, fileName,
                            ext);
                    }
                }
                if (faculty.JoiningReportDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(joiningreportpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(joiningreportpath));
                    }

                    var ext = Path.GetExtension(faculty.JoiningReportDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.JoiningReportDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(joiningreportpath), fileName, ext));
                        UpdatedFaculty.JoiningOrder = string.Format("{0}/{1}{2}", joiningreportpath, fileName, ext);
                    }
                }
                //phd Undertaking Doc
                if (faculty.PhdUndertakingDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(phdundertakingdocpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(phdundertakingdocpath));
                    }

                    var ext = Path.GetExtension(faculty.PhdUndertakingDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.PhdUndertakingDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(phdundertakingdocpath), fileName, ext));
                        UpdatedFaculty.PHDUndertakingDocument = string.Format("{0}/{1}{2}", phdundertakingdocpath, fileName, ext);
                    }
                }
                UpdatedFaculty.createdOn = DateTime.Now;
                UpdatedFaculty.createdBy = userID;
                db.jntuh_appeal_principal_registered.Add(UpdatedFaculty);
                db.SaveChanges();
                TempData["Success"] = "Principal Registration Number Successfully Added.";
                TempData["Error"] = null;
            }
            else
            {
                TempData["Error"] = "Principal Registration Number is already appealed";
            }
            return RedirectToAction("Appelaprincipal");
        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult ComplicanceprincipalDetails()
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var facultydetails = new List<jntuh_appeal_principal_registered>();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    //return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            if (collegeId != 0)
            {
                facultydetails = db.jntuh_appeal_principal_registered.Where(i => i.collegeId == collegeId && i.academicYearId == prAy).Select(i => i).ToList();
            }
            return View(facultydetails);
        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult Appealprincipalreverification(string collegeId, string registrationnumber)
        {
            List<FacultyRegistration> facultyDetails = new List<FacultyRegistration>();
            ViewBag.DeactivationReason = null;
            if (!string.IsNullOrEmpty(collegeId))
            {
                int CollegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();
                var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == CollegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

                var currentDate = DateTime.Now;
                DateTime EditFromDate;
                DateTime EditTODate;
                int collegeid = 0;
                bool PageEdible = false;
                if (CollegeDetails != null)
                {
                    EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                    EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                    collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                    PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                    if (currentDate >= EditFromDate && currentDate <= EditTODate)
                    {
                        if (PageEdible == false)
                        {
                            return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                        }
                    }
                    else
                    {
                        return RedirectToAction("CollegeDashboard", "Dashboard");
                        //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                }
                //Reg nos related online facultyIds
                var rg = db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == registrationnumber).FirstOrDefault();

                if (rg != null)
                {
                    string Reason = null;
                    FacultyRegistration FR = new FacultyRegistration();
                    FR.RegistrationNumber = rg.RegistrationNumber;
                    FR.id = rg.id;
                    FR.CollegeId = CollegeId;
                    FR.DepartmentId = rg.DepartmentId;
                    FR.department = db.jntuh_department.Where(i => i.id == rg.DepartmentId).Select(i => i.departmentName).FirstOrDefault();
                    FR.FirstName = rg.FirstName + rg.MiddleName + rg.LastName;
                    FR.facultyPhoto = rg.Photo;
                    FR.DeactivationReason = rg.DeactivationReason;

                    FR.Type = rg.type;
                    FR.Absent = rg.Absent != null ? (bool)rg.Absent : false;
                    FR.NOTQualifiedAsPerAICTE = rg.NotQualifiedAsperAICTE != null ? (bool)rg.NotQualifiedAsperAICTE : false;
                    FR.InvalidPANNo = rg.InvalidPANNumber != null ? (bool)rg.InvalidPANNumber : false;
                    FR.InCompleteCeritificates = rg.IncompleteCertificates != null ? (bool)rg.IncompleteCertificates : false;
                    FR.PANNumber = rg.PANNumber;
                    FR.XeroxcopyofcertificatesFlag = rg.Xeroxcopyofcertificates != null ? (bool)rg.Xeroxcopyofcertificates : false;
                    FR.NOrelevantUgFlag = rg.NoRelevantUG == "Yes" ? true : false;
                    FR.NOrelevantPgFlag = rg.NoRelevantPG == "Yes" ? true : false;
                    FR.NOrelevantPhdFlag = rg.NORelevantPHD == "Yes" ? true : false;
                    FR.BlacklistFaculty = rg.Blacklistfaculy != null ? (bool)rg.Blacklistfaculy : false;
                    FR.NotIdentityFiedForAnyProgramFlag = rg.NotIdentityfiedForanyProgram != null ? (bool)rg.NotIdentityfiedForanyProgram : false;
                    FR.OriginalCertificatesnotshownFlag = rg.OriginalCertificatesNotShown != null ? (bool)rg.OriginalCertificatesNotShown : false;
                    //FR.NoSCM = rg.NoSCM != null ? (bool)rg.NoSCM : false;
                    //FR.SamePANUsedByMultipleFaculty = rg.SamePANUsedByMultipleFaculty != null ? (bool)(rg.SamePANUsedByMultipleFaculty) : false;
                    FR.InvalidAadhaar = rg.InvalidAadhaar;
                    FR.Basstatus = rg.BAS;
                    FR.OriginalsVerifiedUG = rg.OriginalsVerifiedUG == true ? true : false;
                    FR.OriginalsVerifiedPHD = rg.OriginalsVerifiedPHD == true ? true : false;
                    FR.GenuinenessnotSubmitted = rg.Genuinenessnotsubmitted == true ? true : false;
                    FR.InvalidDegree = rg.Invaliddegree == true ? true : false;
                    FR.NotconsiderPhd = rg.NotconsideredPHD == true ? true : false;
                    FR.NoPgSpecialization = rg.NoPGspecialization == true ? true : false;

                    FR.jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(i => i.facultyId == rg.id).ToList();


                    if (FR.Absent == true)
                        Reason += "Absent";
                    if (FR.Type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }

                    if (FR.XeroxcopyofcertificatesFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Xerox copyof certificates";
                        else
                            Reason += "Xerox copyof certificates";
                    }

                    if (FR.NOrelevantUgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant UG";
                        else
                            Reason += "NO Relevant UG";
                    }

                    if (FR.NOrelevantPgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PG";
                        else
                            Reason += "NO Relevant PG";
                    }
                    if (FR.GenuinenessnotSubmitted == true)
                    {
                        if (Reason != null)
                            Reason += ",GenuinenessnotSubmitted";
                        else
                            Reason += "GenuinenessnotSubmitted";
                    }
                    if (FR.InvalidDegree == true)
                    {
                        if (Reason != null)
                            Reason += ",InvalidDegree";
                        else
                            Reason += "InvalidDegree";
                    }
                    if (FR.NotconsiderPhd == true)
                    {
                        if (Reason != null)
                            Reason += ",Not consider PHD";
                        else
                            Reason += "Not consider PHD";
                    }
                    if (FR.NoPgSpecialization == true)
                    {
                        if (Reason != null)
                            Reason += ",No PG Specialization";
                        else
                            Reason += "No PG Specialization";
                    }
                    if (FR.NOrelevantPhdFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PHD";
                        else
                            Reason += "NO Relevant PHD";
                    }

                    if (FR.NotIdentityFiedForAnyProgramFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NOT Qualified AsPerAICTE";
                        else
                            Reason += "NOT Qualified AsPerAICTE";
                    }

                    if (FR.InvalidPANNo == true)
                    {
                        if (Reason != null)
                            Reason += ",InvalidPANNumber";
                        else
                            Reason += "InvalidPANNumber";
                    }

                    if (FR.InCompleteCeritificates == true)
                    {
                        if (Reason != null)
                            Reason += ",InComplete Ceritificates";
                        else
                            Reason += "InComplete Ceritificates";
                    }

                    if (FR.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",NoSCM";
                        else
                            Reason += "NoSCM";
                    }

                    if (FR.OriginalCertificatesnotshownFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Original Certificates notshown";
                        else
                            Reason += "Original Certificates notshown";
                    }

                    if (FR.PANNumber == null)
                    {
                        if (Reason != null)
                            Reason += ",No PANNumber";
                        else
                            Reason += "No PANNumber";
                    }

                    //if (FR.NotIdentityFiedForAnyProgramFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NotIdentityFied ForAnyProgram";
                    //    else
                    //        Reason += "NotIdentityFied ForAnyProgram";
                    //}

                    //if (FR.SamePANUsedByMultipleFaculty == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",SamePANUsedByMultipleFaculty";
                    //    else
                    //        Reason += "SamePANUsedByMultipleFaculty";
                    //}

                    if (FR.Basstatus == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",No/Invalid Aadhaar Document";
                        else
                            Reason += "No/Invalid Aadhaar Document";
                    }

                    if (FR.BasstatusOld == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",BAS Flag";
                        else
                            Reason += "BAS Flag";
                    }

                    if (FR.OriginalsVerifiedUG == true)
                    {
                        if (Reason != null)
                            Reason += ",Complaint PHD Faculty";
                        else
                            Reason += "Complaint PHD Faculty";
                    }

                    if (FR.OriginalsVerifiedPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",No Guide Sign in PHD Thesis";
                        else
                            Reason += "No Guide Sign in PHD Thesis";
                    }
                    if (FR.BlacklistFaculty == true)
                    {
                        if (Reason != null)
                            Reason += ",Blacklistfaculy";
                        else
                            Reason += "Blacklistfaculy";
                    }
                    if (FR.DeactivationReason != null)
                    {
                        if (Reason != null)
                            Reason += "," + FR.DeactivationReason;
                        else
                            Reason += FR.DeactivationReason;
                    }


                    facultyDetails.Add(FR);
                    ViewBag.collegeid = rg.collegeId;
                    ViewBag.Rgno = rg.RegistrationNumber;
                    ViewBag.DeactivationReason = Reason;


                }
            }
            return View(facultyDetails);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult Reverificationprincipal(string collegeId, string fid, int deficencycount, int departmentid, string degree, string specializationid, string registrationnumber)
        {
            int facultyId = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            if (!string.IsNullOrEmpty(fid))
            {
                facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.registrationnumber = registrationnumber;
            CollegeFaculty faculty = new CollegeFaculty();
            faculty.FacultyRegistrationNumber = registrationnumber;
            faculty.collegeId = Convert.ToInt16(collegeId);
            faculty.DegreeName = degree;
            faculty.SpecializationId = Convert.ToInt16(specializationid);
            faculty.facultyDepartmentId = departmentid;
            faculty.Facultydeficencycount = deficencycount;
            faculty.AadhaarFlag =
                db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == registrationnumber.Trim())
                    .Select(s => s.InvalidAadhaar)
                    .FirstOrDefault();
            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult Reverificationprincipal(CollegeFaculty faculty)
        {
            TempData["Error"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var jntuh_appeal_faculty = db.jntuh_appeal_principal_registered.Where(r => r.academicYearId == prAy).ToList();
            var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
            string[] RegistrationNumber = jntuh_appeal_faculty.Select(i => i.RegistrationNumber).ToArray();

            var physicalpresencpath = "~/Content/Upload/OnlineAppealDocuments/Principal/PhysicalPresenceReports";
            var phdundertakingdocpath = "~/Content/Upload/OnlineAppealDocuments/Principal/PhdUndertakingReports";
            var PrinicipalAadhaarDocument = "~/Content/Upload/OnlineAppealDocuments/Principal/AadhaarDocuments";
            //var PrinicipalAadhaarDocument = "~/Content";
            if (!RegistrationNumber.Contains(faculty.FacultyRegistrationNumber.Trim()))
            {
                //int FacultyId = db.jntuh_college_faculty_registered.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(F => F.id).FirstOrDefault();
                //jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(i => i).FirstOrDefault();

                jntuh_appeal_principal_registered UpdatedFaculty = new jntuh_appeal_principal_registered();
                UpdatedFaculty.collegeId = isRegisteredFaculty.collegeId != null
                    ? (int)isRegisteredFaculty.collegeId
                    : 0;
                UpdatedFaculty.collegeId = faculty.collegeId;
                UpdatedFaculty.academicYearId = prAy;
                UpdatedFaculty.RegistrationNumber = faculty.FacultyRegistrationNumber.Trim();
                UpdatedFaculty.existingFacultyId = isRegisteredFaculty.id;

                UpdatedFaculty.AadhaarNumber = faculty.facultyAadhaarNumber == null ? faculty.facultyAadhaarNumber : faculty.facultyAadhaarNumber.Trim();

                if (faculty.facultyAadharPhotoDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(PrinicipalAadhaarDocument)))
                    {
                        Directory.CreateDirectory(Server.MapPath(PrinicipalAadhaarDocument));
                    }

                    var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);
                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(PrinicipalAadhaarDocument),
                            fileName, ext));
                        UpdatedFaculty.AadhaarDocument = string.Format("{0}/{1}{2}", PrinicipalAadhaarDocument, fileName, ext);
                    }
                }
                if (!String.IsNullOrEmpty(faculty.facultyAadharDocument))
                {
                    UpdatedFaculty.AadhaarDocument = faculty.facultyAadharDocument;
                }
                if (faculty.PhysicalPresenceDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(physicalpresencpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(physicalpresencpath));
                    }

                    var ext = Path.GetExtension(faculty.PhysicalPresenceDocument.FileName);
                    if (ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.PhysicalPresenceDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(physicalpresencpath),
                            fileName, ext));
                        UpdatedFaculty.PhysicalPresenceonInspection = string.Format("{0}/{1}{2}", physicalpresencpath, fileName, ext);
                    }
                }
                //pdf saving code 
                if (faculty.PhdUndertakingDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(phdundertakingdocpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(phdundertakingdocpath));
                    }
                    var ext1 = Path.GetExtension(faculty.PhdUndertakingDocument.FileName);
                    if (ext1.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.PhdUndertakingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(phdundertakingdocpath),
                            fileName, ext1));
                        UpdatedFaculty.PHDUndertakingDocument = string.Format("{0}/{1}{2}", phdundertakingdocpath, fileName, ext1);
                    }
                }
                UpdatedFaculty.createdOn = DateTime.Now;
                UpdatedFaculty.createdBy = userID;
                db.jntuh_appeal_principal_registered.Add(UpdatedFaculty);
                db.SaveChanges();
                TempData["Success"] = "Principal Registration Number Successfully Added for Re-verification.";
                TempData["Error"] = null;
            }

            else
            {
                var facultydata =
                    db.jntuh_appeal_principal_registered.Where(
                        i => i.RegistrationNumber == faculty.FacultyRegistrationNumber && i.academicYearId == prAy).FirstOrDefault();

                facultydata.AadhaarNumber = faculty.facultyAadhaarNumber;
                facultydata.academicYearId = prAy;
                if (faculty.facultyAadharPhotoDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(PrinicipalAadhaarDocument)))
                    {
                        Directory.CreateDirectory(Server.MapPath(PrinicipalAadhaarDocument));
                    }

                    var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);
                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(PrinicipalAadhaarDocument),
                            fileName, ext));
                        facultydata.AadhaarDocument = string.Format("{0}/{1}{2}", PrinicipalAadhaarDocument, fileName, ext);
                    }
                }

                if (faculty.PhysicalPresenceDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(physicalpresencpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(physicalpresencpath));
                    }

                    var ext = Path.GetExtension(faculty.PhysicalPresenceDocument.FileName);
                    if (ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.PhysicalPresenceDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(physicalpresencpath),
                            fileName, ext));
                        facultydata.PhysicalPresenceonInspection = string.Format("{0}/{1}{2}", physicalpresencpath, fileName, ext);
                    }
                    //pdf saving code 

                    if (faculty.PhdUndertakingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(phdundertakingdocpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(phdundertakingdocpath));
                        }

                        var ext1 = Path.GetExtension(faculty.PhdUndertakingDocument.FileName);
                        if (ext1.ToUpper().Equals(".PDF"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.PhdUndertakingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(phdundertakingdocpath),
                                fileName, ext1));
                            facultydata.PHDUndertakingDocument = string.Format("{0}/{1}{2}", phdundertakingdocpath, fileName, ext1);
                        }
                    }
                    facultydata.updatedBy = userID;
                    facultydata.updatedOn = DateTime.Now;
                    db.Entry(facultydata).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Faculty Registration Number Successfully updated for Re-verification..";
                    TempData["Error"] = null;
                }
            }
            //return RedirectToAction("Appealprincipalreverification", "CollegeAppeal", new { @collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]), @registrationnumber = faculty.FacultyRegistrationNumber });
            return RedirectToAction("ComplicanceprincipalDetails");
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult ViewCollegeFacultyWithIntake(string type)
        {
            #region new code
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //collegeId = 42;
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    // return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            ViewBag.IscollegeEditable = db.jntuh_appeal_college_edit_status.Where(i => i.collegeId == collegeId).Select(i => i.IsCollegeEditable).FirstOrDefault();
            #region


            // Principal Details
            string strPrincipalRegno = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf.RegistrationNumber).FirstOrDefault();

            var principal = string.Empty;
            var Reason = string.Empty;
            var prinicpalexists = false;
            //Reg nos related online facultyIds
            var regdata = db.jntuh_registered_faculty.FirstOrDefault(rf => strPrincipalRegno == rf.RegistrationNumber);

            if (regdata != null)
            {
                if (!string.IsNullOrEmpty(regdata.DeactivationReason))
                    Reason = regdata.DeactivationReason;
                //if (regdata.Absent == true)
                //{
                //    Reason = "NOT AVAILABLE" + ",";
                //}
                //if (regdata.NotQualifiedAsperAICTE == true)
                //{
                //    Reason += "NOT QUALIFIED " + ",";
                //}
                //if (regdata.InvalidPANNumber == true)
                //{
                //    Reason += "NO PAN" + ",";
                //}
                //if (regdata.FalsePAN == true)
                //{
                //    Reason += "FALSE PAN" + ",";
                //}
                //if (regdata.NoSCM == true)
                //{
                //    Reason += "NO SCM/RATIFICATION" + ",";
                //}


                if (regdata.BAS == "Yes")
                {
                    if (!String.IsNullOrEmpty(Reason))
                        Reason += ",Not Fulfilling Biometric Attendance";
                    else
                        Reason += "Not Fulfilling Biometric Attendance";
                }

                if (Reason != "")
                {
                    Reason = Reason;
                }
                else
                {
                    Reason = "Dr. " + regdata.FirstName.First().ToString().ToUpper() + regdata.FirstName.Substring(1) + " " + regdata.LastName.First().ToString().ToUpper() + regdata.LastName.Substring(1);
                    prinicpalexists = true;
                }
            }
            else
            {
                Reason = "NO PRINCIPAL";
            }

            ViewBag.PrincipalRegno = Reason;
            if (prinicpalexists == true)

                ViewBag.PrincipalDeficiency = "NO Principal Deficiency";

            else
                ViewBag.PrincipalDeficiency = "Principal Deficiency";
            #endregion


            #region Faculty
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();

            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            if (collegeId != null)
            {
                int userCollegeID = (int)collegeId;
                var jntuh_specialization = db.jntuh_specialization.ToList();
                int[] collegeIDs = null;
                int facultystudentRatio = 0;
                decimal facultyRatio = 0m;
                if (collegeId != 0)
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                }
                else
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
                }
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
                var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
                var jntuh_degree = db.jntuh_degree.ToList();

                int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
                var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();


                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();
                foreach (var item in intake)
                {
                    CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.isActive = item.isActive;
                    newIntake.nbaFrom = item.nbaFrom;
                    newIntake.nbaTo = item.nbaTo;
                    newIntake.specializationId = item.specializationId;
                    newIntake.Specialization = item.jntuh_specialization.specializationName;
                    newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                    newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                    newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                    newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                    newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
                    collegeIntakeExisting.Add(newIntake);
                }
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();


                #region old Code


                ////college Reg nos
                //var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                //string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                //var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                ////education categoryIds UG,PG,PHD...........
                //var jntuh_education_category = db.jntuh_education_category.ToList();
                //int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
                ////Reg nos related online facultyIds
                //var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                //   : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& (rf.collegeId == null || rf.collegeId == collegeId)
                ////Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                //var regfacultywithoutdepts = registeredFaculty.Where(r => r.DepartmentId == null).Select(i => i.type);

                //var jntuh_registered_faculty1 = registeredFaculty.Where(rf => ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                //                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && (rf.PHDundertakingnotsubmitted != true)
                //                                        && (rf.Notin116 != true) && (rf.Blacklistfaculy != true))).Select(rf => new
                //                                        {
                //                                            RegistrationNumber = rf.RegistrationNumber,
                //                                            Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
                //                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                //                                            IsApproved = rf.isApproved,
                //                                            PanNumber = rf.PANNumber,
                //                                            AadhaarNumber = rf.AadhaarNumber,
                //                                            TotalExperience = rf.TotalExperience,
                //                                            jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education
                //                                        }).ToList();
                //jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                //var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                //{
                //    RegistrationNumber = rf.RegistrationNumber,
                //    Department = rf.Department,
                //    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                //    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                //    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                //    PanNumber = rf.PanNumber,
                //    AadhaarNumber = rf.AadhaarNumber,
                //    TotalExperience = rf.TotalExperience,
                //  registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
                //}).ToList();

                #endregion


                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //education categoryIds UG,PG,PHD...........

                var jntuh_specializations = db.jntuh_specialization.ToList();
                var jntuh_departments = db.jntuh_department.ToList();
                int pharmacyDeptId = jntuh_departments.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                var jntuh_education_category = db.jntuh_education_category.ToList();

                //var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                //    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();


                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                var scm = registeredFaculty.Where(i => i.NoSCM == true).ToList();
                //Reg nos related online facultyIds
                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                                                      && (rf.NoSCM == false || rf.NoSCM == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && rf.BAS != "Yes") && rf.InvalidAadhaar != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27))
                                                        .Select(rf => new
                                                        {
                                                            RegistrationNumber = rf.RegistrationNumber,
                                                            Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
                                                            //Department=
                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(s => s.educationId != 8).Select(e => e.educationId).Max() : 0,
                                                            IsApproved = rf.isApproved,
                                                            PanNumber = rf.PANNumber,
                                                            AadhaarNumber = rf.AadhaarNumber,
                                                            PGSpecializationId = rf.PGSpecialization,
                                                            UGDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.DepartmentId).FirstOrDefault(),
                                                            SpecializationId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.SpecializationId).FirstOrDefault(),
                                                            jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education
                                                        }).ToList();
                //var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                //                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && rf.BASStatusOld != "Yes") && rf.BASStatus != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27))
                //                                        .Select(rf => new
                //                                        {
                //                                            RegistrationNumber = rf.RegistrationNumber,
                //                                            Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
                //                                            DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                //                                            SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                //                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                //                                            IsApproved = rf.isApproved,
                //                                            PanNumber = rf.PANNumber,
                //                                            PGSpecializationId = rf.PGSpecialization,
                //                                            AadhaarNumber = rf.AadhaarNumber,
                //                                            UGDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.DepartmentId).FirstOrDefault(),
                //                                            jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education,
                //                                            TotalExperience = rf.TotalExperience
                //                                        }).ToList();

                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    //Department=rf.UGDepartmentId!=null?jntuh_departments.Where(D=>D.id==rf.UGDepartmentId).Select(D=>D.departmentName).FirstOrDefault():"",
                    PGSpecializationId = rf.PGSpecializationId,
                    UGDepartmentId = rf.UGDepartmentId,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    //registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
                    registered_faculty_specialization = rf.SpecializationId != null ? jntuh_specializations.Where(S => S.id == rf.SpecializationId).Select(S => S.specializationName).FirstOrDefault() : rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : "",
                }).ToList();







                var Bpharmacyintake = 0;
                decimal BpharmacyrequiredFaculty = 0;
                foreach (var item in collegeIntakeExisting)
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty = 0;
                    int pgFaculty = 0;
                    int ugFaculty = 0;





                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;

                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);

                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    if (item.Degree == "B.Tech")
                    {
                        int senondyearpercentage = 0;
                        int thirdyearpercentage = 0;
                        int fourthyearpercentage = 0;
                        if (item.admittedIntake1 != 0)
                        {
                            senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(item.admittedIntake1) / Convert.ToDecimal(item.approvedIntake1)) * 100));
                        }
                        if (item.admittedIntake2 != 0)
                        {
                            thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(item.admittedIntake2) / Convert.ToDecimal(item.approvedIntake2)) * 100));
                        }
                        if (item.admittedIntake3 != 0)
                        {
                            fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(item.admittedIntake3) / Convert.ToDecimal(item.approvedIntake3)) * 100));
                        }

                        if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                        {
                            item.ispercentage = true;
                            //studentcount
                            if ((item.admittedIntake1 >= 15 || item.admittedIntake2 >= 15 || item.admittedIntake3 >= 15) && item.ProposedIntake != 0)
                            {
                                item.ispercentage = false;
                                item.isintakeediable = false;
                                //intakedetails.ReducedInatke = 60;
                                //if (intakedetails.approvedIntake1 != 60)
                                //{
                                //    intakedetails.Note = "(Reduced from Proposed Intake of <b>";
                                //    intakedetails.Note += intakedetails.approvedIntake1;
                                //    intakedetails.Note += "</b> as per 25% Clause)";
                                //    intakedetails.approvedIntake1 = 60;
                                //}
                            }
                        }

                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                                                    (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 15;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        var total = intakedetails.totalIntake / 4;
                        Bpharmacyintake = total;
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else //MAM MTM Pharm.D Pharm.D PB
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }

                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

                    string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
                    if (strdegreetype == "UG")
                    {
                        if (item.Degree == "B.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D PB" && f.Recruitedfor == "UG");
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                        }
                    }
                    if (strdegreetype == "PG")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" &&
                                f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department &&
                                        f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                    }

                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);
                    }
                    if (item.Degree == "B.Pharmacy")
                    {
                        BpharmacyrequiredFaculty = Math.Round(intakedetails.requiredFaculty);
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        //intakedetails.Department = "Pharmacy";
                    }
                    if (item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) &&
                                    f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount =registeredFaculty.Where(f =>f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null &&
                        //            (f.isApproved == null || f.isApproved == true)).Count();
                        //intakedetails.Department = "Pharmacy";
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D PB";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

                    }


                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    //=============//

                    intakedetails.FacultyWithIntakeReports = new List<CollegeFacultyWithIntakeReport>();
                    intakedetailsList.Add(intakedetails);
                }
                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
                int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
                // var jntuh_departments = db.jntuh_department.ToList();
                if (btechdegreecount != 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                        var deparmentid = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.departmentId).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Where(f => f.Department == department && f.HighestDegree == "UG").Count();
                        int pgFaculty = jntuh_registered_faculty.Where(f => (f.HighestDegree == "PG" || f.HighestDegree == "M.Phil") && f.Department == department).Count();
                        int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();
                        intakedetailsList.Add(new CollegeFacultyWithIntakeReport
                        {
                            collegeId = (int)collegeId,
                            Degree = "B.Tech",
                            Department = department,
                            Specialization = department,
                            ugFaculty = ugFaculty,
                            pgFaculty = pgFaculty,
                            phdFaculty = phdFaculty,
                            totalFaculty = ugFaculty + pgFaculty + phdFaculty,
                            specializationId = speId,
                            shiftId = 1,
                            DepartmentID = department != "Others" ? deparmentid : 60,
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname)
                        });

                    }
                }

                List<CollegeFacultyWithIntakeReport> facultyCounts = intakedetailsList.Where(i => i.shiftId == 1).ToList();
                int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
                var degrees = db.jntuh_degree.ToList();
                var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 150);
                int remainingFaculty = 0;
                int remainingPHDFaculty = 0;
                decimal departmentWiseRequiredFaculty = 0;
                var distDeptcount = 1;
                var deptloop = 1;
                foreach (var item in intakedetailsList)
                {

                    var SpecializationwisePHDFaculty = 0;
                    if (item.Degree == "M.Tech" || item.Degree == "B.Tech")
                        SpecializationwisePHDFaculty = intakedetailsList.Where(D => D.Department == item.Department && D.Degree == "M.Tech" && D.shiftId == 1).Distinct().Count();
                    else if (item.Degree == "MCA")
                        SpecializationwisePHDFaculty = intakedetailsList.Where(D => D.Department == item.Department && D.Degree == "MCA" && D.shiftId == 1).Distinct().Count();
                    else if (item.Degree == "MBA")
                        SpecializationwisePHDFaculty = intakedetailsList.Where(D => D.Department == item.Department && D.Degree == "MBA" && D.shiftId == 1).Distinct().Count();
                    SpecializationwisePHDFaculty = SpecializationwisePHDFaculty * 2;

                    distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

                    int indexnow = facultyCounts.IndexOf(item);

                    if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                    {
                        deptloop = 1;
                    }

                    departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                    string minimumRequirementMet = string.Empty;
                    int facultyShortage = 0;
                    int adjustedFaculty = 0;
                    int adjustedPHDFaculty = 0;

                    int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));//item.totalFaculty
                    int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                    if (strOtherDepartments.Contains(item.Department))
                    {
                        rFaculty = (int)firstYearRequired;
                        departmentWiseRequiredFaculty = (int)firstYearRequired;
                    }

                    var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                    if (deptloop == 1)
                    {
                        if (rFaculty <= tFaculty)
                        {
                            minimumRequirementMet = "YES";
                            item.deficiency = false;
                            remainingFaculty = tFaculty - rFaculty;
                            adjustedFaculty = rFaculty;//tFaculty
                            item.BtechAdjustedFaculty = adjustedFaculty;
                        }
                        else
                        {
                            minimumRequirementMet = "NO";
                            item.deficiency = true;
                            adjustedFaculty = tFaculty;
                            item.BtechAdjustedFaculty = adjustedFaculty;
                            facultyShortage = rFaculty - tFaculty;
                        }

                        remainingPHDFaculty = item.phdFaculty;

                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")))
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = false;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 0;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = false);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                        else if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")))
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = true;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 2;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = true);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }
                    else
                    {
                        if (rFaculty <= remainingFaculty)
                        {
                            minimumRequirementMet = "YES";
                            item.deficiency = false;
                            if (rFaculty <= item.specializationWiseFaculty)
                            {
                                remainingFaculty = remainingFaculty - rFaculty;
                                adjustedFaculty = rFaculty;
                                item.BtechAdjustedFaculty = adjustedFaculty;
                            }

                            else if (rFaculty >= item.specializationWiseFaculty)
                            {
                                remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                adjustedFaculty = item.specializationWiseFaculty;
                                item.deficiency = true;
                                item.BtechAdjustedFaculty = adjustedFaculty;
                            }
                        }
                        else
                        {
                            minimumRequirementMet = "NO";
                            item.deficiency = true;
                            adjustedFaculty = remainingFaculty;
                            item.BtechAdjustedFaculty = adjustedFaculty;
                            facultyShortage = rFaculty - remainingFaculty;
                            remainingFaculty = 0;
                        }
                        remainingPHDFaculty = item.phdFaculty;
                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")))
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = false;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 0;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = false);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                        else if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")))
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = true;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 2;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = true);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }
                    //else if (degreeType.Equals("PG") && item.approvedIntake1 > 0)
                    //{
                    //    item.PHDdeficiency = true;
                    //    item.AvailablePHDFaculty = 1;
                    //}
                    //else
                    //{
                    //    item.PHDdeficiency = false;
                    //}
                    if (strOtherDepartments.Contains(item.Department))
                    {
                        item.totalIntake = totalBtechFirstYearIntake;
                        item.requiredFaculty = Math.Ceiling((decimal)firstYearRequired);
                    }

                    deptloop++;
                }
            #endregion

                if (Bpharmacyintake >= 100)
                {
                    BpharmacyrequiredFaculty = Math.Round(BpharmacyrequiredFaculty) - 0;
                    ViewBag.BpharmacyrequiredFaculty = BpharmacyrequiredFaculty;
                }
                else
                {
                    BpharmacyrequiredFaculty = Math.Round(BpharmacyrequiredFaculty) - 0;
                    ViewBag.BpharmacyrequiredFaculty = BpharmacyrequiredFaculty;
                }
                intakedetailsList.Where(i => i.PharmacyGroup1 != "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.requiredFaculty = BpharmacyrequiredFaculty);


                #region For Pharmacyview
                var randomcode = "";
                if (collegeId != null)
                {
                    randomcode = db.jntuh_college_randamcodes.FirstOrDefault(i => i.CollegeId == collegeId).RandamCode;
                }
                var pharmadTotalintake = 0;
                var pharmadPBTotalintake = 0;
                var bpharmacyintake = 0;
                decimal BpharcyrequiredFaculty = 0;
                decimal PharmDrequiredFaculty = 0;
                decimal PharmDPBrequiredFaculty = 0;
                foreach (var item in collegeIntakeExisting)
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty = 0;
                    int pgFaculty = 0;
                    int ugFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.collegeRandomCode = randomcode;
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;

                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    if (item.Degree == "B.Tech")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                                                    (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {

                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 15;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        var total = intakedetails.totalIntake / 4;
                        bpharmacyintake = total;
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        intakedetails.totalIntake = pharmadTotalintake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        intakedetails.totalIntake = pharmadPBTotalintake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else //MAM MTM Pharm.D Pharm.D PB
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }

                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

                    string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
                    if (strdegreetype == "UG")
                    {
                        if (item.Degree == "B.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
                        }
                    }

                    if (strdegreetype == "PG")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
                            switch (item.specializationId)
                            {
                                case 114://Hospital & Clinical Pharmacy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice/Pharm D";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP" || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization == "PHARMD".ToUpper() || f.registered_faculty_specialization == "PHARM D" || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
                                    break;
                                case 116://Pharmaceutical Analysis & Quality Assurance
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharma Chemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA" || f.registered_faculty_specialization == "PA RA" || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
                                    break;
                                case 118://Pharmaceutical Management & Regulatory Affaires
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PMRA/Regulatory Affairs/Pharmaceutics";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PMRA".ToUpper() || f.registered_faculty_specialization == "Regulatory Affairs".ToUpper() || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
                                    break;
                                case 120://Pharmaceutics
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
                                    break;
                                case 122://Pharmacology
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP".ToUpper() || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
                                    break;
                                case 124://Quality Assurance & Pharma Regulatory Affairs
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
                                    var s = jntuh_registered_faculty.Where(f => (f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() ||
                                                 f.registered_faculty_specialization == "QA".ToUpper() ||
                                                 f.registered_faculty_specialization == "PA RA".ToUpper() ||
                                                 f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA"))).ToList();
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                                    break;
                                case 115://Industrial Pharmacy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()));
                                    break;
                                case 121://Pharmacognosy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacognosy/Pharma Biotechnology/Pharmacology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization == "Pharmacognosy".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacognosy".ToUpper()));
                                    break;
                                case 117://Pharmaceutical Chemistry
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/Pharmaceuticalchemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()));
                                    break;
                                case 119://Pharmaceutical Technology (2011-12)
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization.Contains("Bio-Technology".ToUpper()) || f.registered_faculty_specialization.Contains("Pharmaceutical Technology (2011-12)".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()));
                                    break;
                                case 123://Quality Assurance
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Quality Assurance".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance".ToUpper()));
                                    break;
                                default:
                                    intakedetails.PharmacySpec1 = "";
                                    intakedetails.PharmacyspecializationWiseFaculty = 0;
                                    break;
                            }
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
                        }
                    }

                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
                    }

                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy")
                    {
                        BpharcyrequiredFaculty = Math.Round(intakedetails.requiredFaculty);
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                        intakedetails.Department = "Pharmacy";

                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                        intakedetails.Department = "Pharmacy";
                        intakedetailsList.Where(i => i.Degree == "M.Pharmacy" && i.specializationId == item.specializationId).ToList().ForEach(c => c.PharmacyspecializationWiseFaculty = intakedetails.PharmacyspecializationWiseFaculty);

                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        PharmDrequiredFaculty = intakedetails.requiredFaculty;
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        PharmDPBrequiredFaculty = intakedetails.requiredFaculty;
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                    }

                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);

                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;

                    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Add(intakedetails);
                }

                #region pharmcy specializations
                var pharmdspeclist = new List<PharmacySpecilaizationList>
                {
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharmacy Practice",
                        Specialization = "Pharm.D"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharm D",
                        Specialization = "Pharm.D"
                    }
                };
                var pharmdpbspeclist = new List<PharmacySpecilaizationList>
                {
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharmacy Practice",
                        Specialization = "Pharm.D PB"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharm D",
                        Specialization = "Pharm.D PB"
                    }
                };

                var pharmacyspeclist = new List<PharmacySpecilaizationList>
                {
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharmaceutics",
                        Specialization = "B.Pharmacy"
                    },
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Industrial Pharmacy",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmacy BioTechnology",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmaceutical Technology",
                    //    Specialization = "B.Pharmacy"
                    //},
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharmaceutical Chemistry",
                        Specialization = "B.Pharmacy"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharmacy Analysis",
                        Specialization = "B.Pharmacy"
                    },

                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "PAQA",
                        Specialization = "B.Pharmacy"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharmacology",
                        Specialization = "B.Pharmacy"
                    },
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharma D",
                    //    Specialization = "B.Pharmacy"
                    //},
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Pharmacognosy",
                        Specialization = "B.Pharmacy"
                    },
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "English",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Mathematics",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Computers",
                    //    Specialization = "B.Pharmacy"
                    //},new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Computer Science",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Zoology",
                    //    Specialization = "B.Pharmacy"
                    //}
                };
                #endregion
                #region All B.Pharmacy Specializations

                var reg_facultyspecilaizationsdistinct = jntuh_registered_faculty.Select(i => i.registered_faculty_specialization).Distinct().ToArray();

                var reg_facultyspecilaizations =
                    jntuh_registered_faculty.Where(
                        i =>
                            i.registered_faculty_specialization == "Pharmaceutics".ToUpper() ||
                            i.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() ||
                            i.registered_faculty_specialization == "Pharmacy BioTechnology".ToUpper() ||
                            i.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() ||
                            i.registered_faculty_specialization == "Pharmacy Analysis".ToUpper() ||
                            i.registered_faculty_specialization == "PAQA".ToUpper() ||
                            i.registered_faculty_specialization == "Pharmacology".ToUpper() ||
                            i.registered_faculty_specialization == "Pharma D".ToUpper() ||
                            i.registered_faculty_specialization == "Pharmacognosy" ||
                            i.registered_faculty_specialization == "English".ToUpper() ||
                            i.registered_faculty_specialization == "Mathematics".ToUpper() ||
                            i.registered_faculty_specialization == "Computers".ToUpper() ||
                            i.registered_faculty_specialization == "Zoology".ToUpper()).ToList();

                var group1Subcount = 0; var group2Subcount = 0; var group3Subcount = 0; var group4Subcount = 0; var group5Subcount = 0; var group6Subcount = 0;
                var pharmadgroup1Subcount = 0; var pharmadPBgroup1Subcount = 0;
                string subgroupconditionsmet;
                string conditionbpharm = null;
                string conditionpharmd = null;
                string conditionphardpb = null;
                foreach (var list in pharmacyspeclist)
                {
                    int phd;
                    int pg;
                    int ug;
                    var bpharmacylist = new CollegeFacultyWithIntakeReport();
                    bpharmacylist.Specialization = list.Specialization;
                    bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                    bpharmacylist.collegeId = (int)collegeId;
                    bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                    bpharmacylist.collegeRandomCode = randomcode;
                    bpharmacylist.shiftId = 1;
                    bpharmacylist.Degree = "B.Pharmacy";
                    bpharmacylist.Department = "Pharmacy";
                    bpharmacylist.PharmacyGroup1 = "Group1";

                    bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
                    bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
                    bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
                    bpharmacylist.totalFaculty = ug + pg + phd;
                    bpharmacylist.BphramacyrequiredFaculty = BpharcyrequiredFaculty;
                    bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;

                    #region bpharmacyspecializationcount

                    if (list.PharmacyspecName == "Pharmaceutics")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }

                    else if (list.PharmacyspecName == "Industrial Pharmacy")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }
                    else if (list.PharmacyspecName == "Pharmacy BioTechnology")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                        f.registered_faculty_specialization == "Bio-Technology".ToUpper());
                    }
                    else if (list.PharmacyspecName == "Pharmaceutical Technology")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper() ||
                            f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
                    }
                    else if (list.PharmacyspecName == "Pharmaceutical Chemistry")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }
                    else if (list.PharmacyspecName == "Pharmacy Analysis")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }

                    else if (list.PharmacyspecName == "PAQA")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                     f.registered_faculty_specialization == "PA & QA".ToUpper() ||
                            //f.registered_faculty_specialization == "Quality Assurance".ToUpper() ||
                            //f.registered_faculty_specialization == "QualityAssurance".ToUpper() ||
                                     f.registered_faculty_specialization == "QAPRA".ToUpper() ||
                                     f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper() ||
                                     f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper());
                    }
                    else if (list.PharmacyspecName == "Pharmacology")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }

                    else if (list.PharmacyspecName == "Pharma D")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                       f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                      f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                      f.registered_faculty_specialization == "Pharm.D".ToUpper());
                    }
                    else if (list.PharmacyspecName == "Pharmacognosy")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                       f.registered_faculty_specialization == "Pharmacognosy & Phytochemistry".ToUpper() ||
                                       f.registered_faculty_specialization == "Pharmacognosy&Phytochemistry".ToUpper());
                    }

                    else if (list.PharmacyspecName == "English")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }
                    else if (list.PharmacyspecName == "Mathematics")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }
                    else if (list.PharmacyspecName == "Computers")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }
                    else if (list.PharmacyspecName == "Computer Science")
                    {
                        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    }
                    //else if (list.PharmacyspecName == "Zoology")
                    //{
                    //    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                    //}
                    #endregion





                    if (list.PharmacyspecName == "Pharmaceutics")//|| list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology"
                    {
                        group1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutics".ToUpper());
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy BioTechnology".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Bio-Technology".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
                        bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 4;
                        bpharmacylist.PharmacySubGroup1 = "SubGroup1";
                    }

                    else if (list.PharmacyspecName == "Pharmaceutical Chemistry" || list.PharmacyspecName == "Pharmacy Analysis" || list.PharmacyspecName == "PAQA" || list.PharmacyspecName == "PA & QA" || list.PharmacyspecName.Contains("QA"))
                    {
                        group2Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()) +
                                         jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
                                         jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
                                         jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
                                         jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
                                         jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
                                         jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                        bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 5;
                        bpharmacylist.PharmacySubGroup1 = "SubGroup2";
                    }
                    //else if (list.PharmacyspecName == "Pharmacy Analysis" || list.PharmacyspecName == "PAQA" || list.PharmacyspecName == "PA & QA" || list.PharmacyspecName.Contains("QA"))
                    //{
                    //    var y = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()).ToList();
                    //    var g = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization.Contains("QA")).ToList();
                    //    var g1 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PAQA".ToUpper()).ToList();
                    //    var g2 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PA & QA".ToUpper()).ToList();

                    //    group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                    //    bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                    //    bpharmacylist.BPharmacySubGroupRequired = 1;
                    //    bpharmacylist.PharmacySubGroup1 = "SubGroup3";
                    //}

                    else if (list.PharmacyspecName == "Pharmacology")//|| list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D"
                    {
                        group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacology".ToUpper());
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
                        //            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper()); ;
                        bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 5 : 4;
                        bpharmacylist.PharmacySubGroup1 = "SubGroup3";
                    }

                    else if (list.PharmacyspecName == "Pharmacognosy")
                    {
                        group4Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy".ToUpper()) +
                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy&Phytochemistryc".ToUpper()) +
                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("PHARMACOGNOSY & PHYTOCHEMISTRY".ToUpper()));
                        bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = 3; ;
                        bpharmacylist.PharmacySubGroup1 = "SubGroup4";
                    }

                    //else if (list.PharmacyspecName == "English" || list.PharmacyspecName == "Mathematics" || list.PharmacyspecName == "Computers" || list.PharmacyspecName == "Computer Science")//|| list.PharmacyspecName == "Zoology"
                    //{
                    //    group6Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "English".ToUpper()) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Mathematics".ToUpper()) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER")) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER SCIENCE")) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("CSE"));
                    //    //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("ZOOLOGY"));
                    //    bpharmacylist.BPharmacySubGroup1Count = group6Subcount;
                    //    if (bpharmacyintake == 100)
                    //    {
                    //        bpharmacylist.BPharmacySubGroupRequired = 3;
                    //    }
                    //    else
                    //    {
                    //        bpharmacylist.BPharmacySubGroupRequired = 2;
                    //    }

                    //    bpharmacylist.PharmacySubGroup1 = "SubGroup6";
                    //}


                    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Add(bpharmacylist);
                }


                //for pharma D specializations
                var pharmaD = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
                if (pharmaD.Count > 0)
                {
                    foreach (var list in pharmdspeclist)
                    {
                        int phd;
                        int pg;
                        int ug;
                        var bpharmacylist = new CollegeFacultyWithIntakeReport();
                        bpharmacylist.Specialization = list.Specialization;
                        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                        bpharmacylist.collegeId = (int)collegeId;
                        bpharmacylist.collegeRandomCode = randomcode;
                        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                        bpharmacylist.shiftId = 1;
                        bpharmacylist.Degree = "Pharm.D";
                        bpharmacylist.Department = "Pharm.D";
                        bpharmacylist.PharmacyGroup1 = "Group1";
                        //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D");
                        bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
                        bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
                        bpharmacylist.totalFaculty = ug + pg + phd;
                        bpharmacylist.pharmadrequiredfaculty = PharmDrequiredFaculty;
                        bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
                        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;

                        #region pharmadSpecializationcount
                        if (list.PharmacyspecName == "Pharm D")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharma D".ToUpper());
                        }
                        else if (list.PharmacyspecName == "Pharmacy Practice")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        #endregion



                        if (list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                        {
                            pharmadgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper());
                            bpharmacylist.BPharmacySubGroup1Count = pharmadgroup1Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = pharmadTotalintake / 30;
                            bpharmacylist.PharmacySubGroup1 = "SubGroup1";
                            intakedetailsList.Where(i => i.Degree == "Pharm.D" && i.Department == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroup1Count = pharmadgroup1Subcount);
                            intakedetailsList.Where(i => i.Degree == "Pharm.D" && i.Department == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupRequired = bpharmacylist.BPharmacySubGroupRequired);
                        }

                        intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Add(bpharmacylist);
                    }
                }


                //for pharma.D PB specializations
                var pharmaDPB = collegeIntakeExisting.Where(i => i.specializationId == 19).ToList();
                if (pharmaDPB.Count > 0)
                {
                    foreach (var list in pharmdpbspeclist)
                    {
                        int phd;
                        int pg;
                        int ug;
                        var bpharmacylist = new CollegeFacultyWithIntakeReport();
                        bpharmacylist.Specialization = list.Specialization;
                        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                        bpharmacylist.collegeId = (int)collegeId;
                        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                        bpharmacylist.collegeRandomCode = randomcode;
                        bpharmacylist.shiftId = 1;
                        bpharmacylist.Degree = "Pharm.D PB";
                        bpharmacylist.Department = "Pharm.D PB";
                        bpharmacylist.PharmacyGroup1 = "Group1";
                        //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB");
                        bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
                        bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
                        bpharmacylist.totalFaculty = ug + pg + phd;
                        bpharmacylist.pharmadPBrequiredfaculty = PharmDPBrequiredFaculty;
                        bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
                        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;

                        #region pharmadPbSpecializationcount
                        if (list.PharmacyspecName == "Pharm D")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharma D".ToUpper());
                        }
                        else if (list.PharmacyspecName == "Pharmacy Practice")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        #endregion



                        if (list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                        {
                            pharmadPBgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
                                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
                                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
                                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
                                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper()); ;
                            bpharmacylist.BPharmacySubGroup1Count = pharmadPBgroup1Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = pharmadPBTotalintake / 10;
                            bpharmacylist.PharmacySubGroup1 = "SubGroup1";
                            intakedetailsList.Where(i => i.Degree == "Pharm.D PB" && i.Department == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroup1Count = pharmadPBgroup1Subcount);
                            intakedetailsList.Where(i => i.Degree == "Pharm.D PB" && i.Department == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupRequired = bpharmacylist.BPharmacySubGroupRequired);
                        }
                        intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Add(bpharmacylist);
                    }
                }

                if (BpharcyrequiredFaculty > 0)
                {
                    if (bpharmacyintake >= 100)
                    {
                        BpharcyrequiredFaculty = Math.Round(BpharcyrequiredFaculty) - 0;
                        ViewBag.BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
                    }
                    else
                    {
                        BpharcyrequiredFaculty = Math.Round(BpharcyrequiredFaculty) - 0;
                        ViewBag.BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
                    }
                    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 != "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.requiredFaculty = BpharcyrequiredFaculty);
                    intakedetailsList.Where(i => i.PharmacyGroup1 != "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.requiredFaculty = BpharcyrequiredFaculty);

                    Group1PharmacyFaculty = group1Subcount; Group2PharmacyFaculty = group2Subcount; Group3PharmacyFaculty = group3Subcount;
                    Group4PharmacyFaculty = group4Subcount; Group5PharmacyFaculty = group5Subcount; Group6PharmacyFaculty = group6Subcount;
                    if (jntuh_registered_faculty.Count >= BpharcyrequiredFaculty)
                    {
                        if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                        {
                            subgroupconditionsmet = conditionbpharm = "No";
                        }
                        else
                        {
                            subgroupconditionsmet = conditionbpharm = "Yes";
                        }
                    }
                    else
                    {
                        subgroupconditionsmet = conditionbpharm = "Yes";
                    }

                    ViewBag.BpharmcyCondition = conditionbpharm;
                    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                }

                if (PharmDrequiredFaculty > 0)
                {
                    if (jntuh_registered_faculty.Count >= PharmDrequiredFaculty)
                    {
                        if (pharmadgroup1Subcount >= pharmadTotalintake / 30)
                        {
                            subgroupconditionsmet = conditionpharmd = "No";
                        }
                        else
                        {
                            subgroupconditionsmet = conditionpharmd = "Yes";
                        }
                    }
                    else
                    {
                        subgroupconditionsmet = conditionpharmd = "Yes";
                    }

                    ViewBag.PharmaDCondition = conditionpharmd;
                    if (conditionbpharm == "No")
                    {
                        intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                    }
                    else
                    {
                        intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
                    }


                }

                ViewBag.PharmDPBrequiredFaculty = PharmDPBrequiredFaculty;
                if (PharmDPBrequiredFaculty > 0)
                {
                    if (jntuh_registered_faculty.Count >= PharmDPBrequiredFaculty)
                    {
                        if (pharmadPBgroup1Subcount >= pharmadPBTotalintake / 10)
                        {
                            subgroupconditionsmet = conditionphardpb = "No";
                        }
                        else
                        {
                            subgroupconditionsmet = conditionphardpb = "Yes";
                        }
                    }
                    else
                    {
                        subgroupconditionsmet = conditionphardpb = "Yes";
                    }

                    ViewBag.PharmaDPBCondition = conditionphardpb;
                    if (conditionbpharm == "No" && conditionpharmd == "No")
                    {
                        intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                    }
                    else
                    {
                        intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
                    }

                }


                intakedetailsList.FirstOrDefault().FacultyWithIntakeReports = intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                #endregion



                #endregion

                #region Faculty Appeal Deficiency Status
                var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
                var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
                foreach (var item in intakedetailsList)
                {
                    var deparment = jntuh_deparment.FirstOrDefault(i => i.id == item.DepartmentID);
                    if (deparment != null)
                    {
                        var jntuh_departmentcount =
                        jntuh_appeal_faculty.Where(
                            i =>
                                i.DepartmentId == item.DepartmentID && i.SpecializationId == item.specializationId &&
                                i.DegreeId == deparment.degreeId && i.collegeId == collegeId).ToList();
                        var facultydefcount = (int)Math.Ceiling(item.requiredFaculty) - item.totalFaculty;

                        if (jntuh_registered_faculty.Count >= BpharcyrequiredFaculty && (item.Department == "B.Pharmacy"))
                        {
                            if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                            {
                                Allgroupscount = 0;
                            }
                            else
                            {
                                //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
                                if (group1Subcount < (bpharmacyintake >= 100 ? 6 : 4))
                                {
                                    var count = (bpharmacyintake >= 100 ? 6 : 4) - group1Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group2Subcount < (bpharmacyintake >= 100 ? 6 : 5))
                                {
                                    var count = (bpharmacyintake >= 100 ? 6 : 5) - group2Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group3Subcount < (bpharmacyintake >= 100 ? 5 : 4))
                                {
                                    var count = (bpharmacyintake >= 100 ? 5 : 4) - group3Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group4Subcount < 3)
                                {
                                    var count = 3 - group4Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                //if (group5Subcount < 2)
                                //{
                                //    var count = 2 - group5Subcount;
                                //    Allgroupscount = Allgroupscount + count;
                                //}
                                //if (group6Subcount < bpharmacyIntake)
                                //{
                                //    var count = bpharmacyIntake - group6Subcount;
                                //    Allgroupscount = Allgroupscount + count;
                                //}
                            }
                            facultydefcount = Allgroupscount;
                        }

                        else if (jntuh_registered_faculty.Count < BpharcyrequiredFaculty && (item.Department == "B.Pharmacy"))
                        {
                            if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                            {
                                Allgroupscount = 0;
                            }
                            else
                            {
                                //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
                                if (group1Subcount < (bpharmacyintake >= 100 ? 6 : 4))
                                {
                                    var count = (bpharmacyintake >= 100 ? 6 : 4) - group1Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group2Subcount < (bpharmacyintake >= 100 ? 6 : 5))
                                {
                                    var count = (bpharmacyintake >= 100 ? 6 : 5) - group2Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group3Subcount < (bpharmacyintake >= 100 ? 5 : 4))
                                {
                                    var count = (bpharmacyintake >= 100 ? 5 : 4) - group3Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group4Subcount < 3)
                                {
                                    var count = 3 - group4Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                //if (group5Subcount < 2)
                                //{
                                //    var count = 2 - group5Subcount;
                                //    Allgroupscount = Allgroupscount + count;
                                //}
                                //if (group6Subcount < bpharmacyIntake)
                                //{
                                //    var count = bpharmacyIntake - group6Subcount;
                                //    Allgroupscount = Allgroupscount + count;
                                //}
                            }

                            var lessfaculty = BpharcyrequiredFaculty - jntuh_registered_faculty.Count;

                            if (lessfaculty > Allgroupscount)
                            {
                                facultydefcount = (int)lessfaculty + Allgroupscount;
                            }
                            else if (Allgroupscount > lessfaculty)
                            {
                                facultydefcount = Allgroupscount + (int)lessfaculty;
                            }
                        }

                        if (item.Department == "B.Pharmacy")
                        {
                            ViewBag.BpharmacyRequired = facultydefcount;
                        }

                        if (item.PharmacyspecializationWiseFaculty < 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
                        {
                            facultydefcount = 1;
                        }

                        if (item.Department == "Pharm.D" || item.Department == "Pharm.D PB")
                        {
                            facultydefcount = item.BPharmacySubGroupRequired - item.BPharmacySubGroup1Count;
                        }
                        if (facultydefcount <= jntuh_departmentcount.Count && jntuh_departmentcount.Count != 0)
                        {
                            item.deficiencystatus = true;
                        }
                    }
                }

                #endregion

                #region For labs
                List<AnonymousLabclass> collegeLabAnonymousLabclass = new List<AnonymousLabclass>();

                int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == 10 && e.courseStatus != "Closure" && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToArray();

                int[] DegreeIDs = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == 4 && specializationIds.Contains(l.SpecializationID)).Select(l => l.DegreeID).ToArray();
                List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                            .Where(l => specializationIds.Contains(l.SpecializationID))
                                                            .Select(l => new Lab
                                                            {
                                                                EquipmentID = l.id,
                                                                degreeId = l.DegreeID,
                                                                degree = l.jntuh_degree.degree,
                                                                degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                                departmentId = l.DepartmentID,
                                                                department = l.jntuh_department.departmentName,
                                                                specializationId = l.SpecializationID,
                                                                specializationName = l.jntuh_specialization.specializationName,
                                                                year = l.Year,
                                                                Semester = l.Semester,
                                                                Labcode = l.Labcode,
                                                                LabName = l.LabName,
                                                                EquipmentName = l.EquipmentName
                                                            })
                                                            .OrderBy(l => l.degreeDisplayOrder)
                                                            .ThenBy(l => l.department)
                                                            .ThenBy(l => l.specializationName)
                                                            .ThenBy(l => l.year).ThenBy(l => l.Semester)
                                                            .ToList();


                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

                List<jntuh_lab_master> jntuh_lab_masters = new List<jntuh_lab_master>();

                if (CollegeAffiliationStatus == "Yes")
                {
                    if (DegreeIDs.Contains(4))
                    {
                        jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => (l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.CollegeId == collegeId && !l.EquipmentName.Contains("desirable")).ToList();
                    }
                    else
                    {
                        jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == collegeId && !l.EquipmentName.Contains("desirable")).ToList();
                    }

                }
                else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
                {

                    if (DegreeIDs.Contains(4))
                    {
                        jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => (l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.CollegeId == null && !l.EquipmentName.Contains("desirable")).ToList();
                    }
                    else
                    {
                        jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == null && !l.EquipmentName.Contains("desirable")).ToList();
                    }
                }

                if (CollegeAffiliationStatus == "Yes")
                {
                    collegeLabAnonymousLabclass = jntuh_lab_masters
                        //db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID))
                                                         .Select(l => new AnonymousLabclass
                                                         {
                                                             id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                                                             EquipmentID = l.id,
                                                             LabName = l.LabName,
                                                             EquipmentName = l.EquipmentName,
                                                             LabCode = l.Labcode,
                                                         })
                                                         .OrderBy(l => l.LabName)
                                                         .ThenBy(l => l.EquipmentName)
                                                         .ToList();

                }
                else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
                {

                    collegeLabAnonymousLabclass = jntuh_lab_masters
                        //db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.Labcode != "TMP-CL")
                                                       .Select(l => new AnonymousLabclass
                                                       {
                                                           id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                                                           EquipmentID = l.id,
                                                           LabName = l.LabName,
                                                           EquipmentName = l.EquipmentName,
                                                           LabCode = l.Labcode,
                                                       })
                                                       .OrderBy(l => l.LabName)
                                                       .ThenBy(l => l.EquipmentName)
                                                       .ToList();
                }

                //   var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeId && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

                var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                //  list1 = list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

                #region this code written by suresh

                int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

                int[] clgequipmentIDs = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeId && labequipmentIds.Contains(l.EquipmentID) && l.isActive == true).Select(i => i.EquipmentID).ToArray();

                list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID)).ToList();

                #endregion
                if (facultyCounts.Count > 0)
                {
                    facultyCounts.FirstOrDefault().LabsListDefs1 = list1.ToList();
                }

                List<PhysicalLabMaster> physicallabs = new List<PhysicalLabMaster>();
                List<UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs> CollegePhysicalLabMaster = new List<UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs>();
                CollegePhysicalLabMaster =
                      db.jntuh_physical_labmaster_copy.Where(e => e.Collegeid == collegeId && e.Numberofrequiredlabs != null)
                          .Select(e => new UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs
                          {
                              department = db.jntuh_department.Where(d => d.id == e.DepartmentId).Select(s => s.departmentName).FirstOrDefault(),
                              NoOfRequiredLabs = e.Numberofrequiredlabs,
                              Labname = e.LabName,
                              year = e.Year,
                              semister = e.Semister,
                              LabCode = e.Labcode,
                              NoOfAvailabeLabs = db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeId && a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault() == null ? 0 : db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeId && a.DepartmentId == e.DepartmentId && a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault()
                          }).ToList();
                foreach (var item in CollegePhysicalLabMaster)
                {
                    if (item.NoOfAvailabeLabs < item.NoOfRequiredLabs)
                    {
                        PhysicalLabMaster PhysicalLabMaster = new PhysicalLabMaster();
                        PhysicalLabMaster.DepartmentName = item.department;
                        PhysicalLabMaster.NoofAvailable = (int)item.NoOfAvailabeLabs;
                        PhysicalLabMaster.NoofRequeried = (int)item.NoOfRequiredLabs;
                        PhysicalLabMaster.Labname = item.Labname;
                        physicallabs.Add(PhysicalLabMaster);
                    }
                }
                string physicalpath = db.jntuh_college_enclosures.Where(e => e.enclosureId == 26 && e.collegeID == collegeId).Select(e => e.path).FirstOrDefault();
                if (!string.IsNullOrEmpty(physicalpath))
                {
                    if (physicallabs.Count != 0)
                    {
                        physicallabs[0].PhysicalLabUploadingview = physicalpath;
                    }

                }
                if (facultyCounts.Count > 0)
                {
                    facultyCounts.FirstOrDefault().PhysicalLabs = physicallabs.ToList();
                }
                #endregion
            }
            //Showing supporting Documents
            jntuh_appeal_college_edit_status jntuh_appeal_college_edit_status =
                db.jntuh_appeal_college_edit_status.Where(e => e.collegeId == collegeId).Select(s => s).FirstOrDefault();
            ViewBag.DeclarationPath =
            jntuh_appeal_college_edit_status.DeclarationPath;
            ViewBag.FurtherAppealSupportingDocument =
          jntuh_appeal_college_edit_status.FurtherAppealSupportingDocument;
            return View(intakedetailsList);
            #endregion
        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult AppealExistingAndProposedIntake()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            #region CollegeEditStatus
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    //return RedirectToAction("College", "Dashboard");
                    return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("College", "Dashboard");
            }
            #endregion

            #region For CollegeIntake

            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            var collegefacultyreport = new CollegeFacultyWithIntakeReport()
            {
                collegeId = userCollegeID
            };
            intakedetailsList.Add(collegefacultyreport);


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
            var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
            var jntuh_degree = db.jntuh_degree.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));
            int studentcount = Convert.ToInt32(ConfigurationManager.AppSettings["studentcount"]);

            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            // var presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            var AYY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            var AYY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            var AYY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            var AYY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            var AYY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
            var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
            int[] collegeIDs = null;


            if (userCollegeID != 0)
            {
                //Commented for just checking
                //collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == userCollegeID).Select(c => c.id).ToArray();

            }

            //Inactive specialization Ids
            int[] inactivespids = db.jntuh_specialization.Where(s => s.isActive == false).Select(s => s.id).ToArray();

            //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId) && i.courseStatus != "Closure" && i.academicYearId == 10 && i.proposedIntake != 0 && !inactivespids.Contains(i.specializationId)).ToList();
            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.courseStatus != "Closure" && i.academicYearId == AY0 && i.proposedIntake != 0 && !inactivespids.Contains(i.specializationId)).ToList();
            foreach (var item in intake)
            {
                CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                newIntake.nbaFrom = item.nbaFrom;
                newIntake.nbaTo = item.nbaTo;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization = item.jntuh_specialization.specializationName;
                newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = item.jntuh_shift.shiftName;
                newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
                collegeIntakeExisting.Add(newIntake);
            }
            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

            var intakelists = new List<CollegeIntakeExisting>();
            foreach (var item in collegeIntakeExisting)
            {
                item.ispercentage = false;
                item.isintakeediable = true;
                if (item.nbaFrom != null)
                    item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                if (item.nbaTo != null)
                    item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

                //FLAG : 1 - Approved, 0 - Admitted

                jntuh_appeal_college_intake_existing detailsappeal = db.jntuh_appeal_college_intake_existing
                                                          .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                                                          .Select(e => e)
                                                          .FirstOrDefault();
                if (detailsappeal != null)
                {
                    item.ApprovedIntake = detailsappeal.approvedIntake;
                    item.letterPath = detailsappeal.approvalLetter;
                    item.ProposedIntake = detailsappeal.proposedIntake;
                    item.courseStatus = detailsappeal.courseStatus;
                }
                else
                {
                    jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                                                          .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                                                          .Select(e => e)
                                                          .FirstOrDefault();

                    if (details != null)
                    {
                        item.ApprovedIntake = details.approvedIntake;
                        item.letterPath = details.approvalLetter;
                        item.ProposedIntake = details.proposedIntake;
                        item.courseStatus = details.courseStatus;
                    }
                }
                //Approved Intake Means JNTU Sanctioned Intake
                item.approvedIntake1 = GetIntake(userCollegeID, AYY1, item.specializationId, item.shiftId, 1);
                item.approvedIntake2 = GetIntake(userCollegeID, AYY2, item.specializationId, item.shiftId, 1);
                item.approvedIntake3 = GetIntake(userCollegeID, AYY3, item.specializationId, item.shiftId, 1);
                item.approvedIntake4 = GetIntake(userCollegeID, AYY4, item.specializationId, item.shiftId, 1);
                item.approvedIntake5 = GetIntake(userCollegeID, AYY5, item.specializationId, item.shiftId, 1);

                item.admittedIntake1 = GetIntake(userCollegeID, AYY1, item.specializationId, item.shiftId, 0);
                item.admittedIntake2 = GetIntake(userCollegeID, AYY2, item.specializationId, item.shiftId, 0);
                item.admittedIntake3 = GetIntake(userCollegeID, AYY3, item.specializationId, item.shiftId, 0);
                item.admittedIntake4 = GetIntake(userCollegeID, AYY4, item.specializationId, item.shiftId, 0);
                item.admittedIntake5 = GetIntake(userCollegeID, AYY5, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake1 = GetIntake(userCollegeID, AYY1, item.specializationId, item.shiftId, 2);
                item.AICTEapprovedIntake2 = GetIntake(userCollegeID, AYY2, item.specializationId, item.shiftId, 2);
                item.AICTEapprovedIntake3 = GetIntake(userCollegeID, AYY3, item.specializationId, item.shiftId, 2);
                item.AICTEapprovedIntake4 = GetIntake(userCollegeID, AYY4, item.specializationId, item.shiftId, 2);
                item.AICTEapprovedIntake5 = GetIntake(userCollegeID, AYY5, item.specializationId, item.shiftId, 2);

                item.ExambranchadmittedIntakeR1 = GetIntake(userCollegeID, AYY1, item.specializationId, item.shiftId, 3);
                item.ExambranchadmittedIntakeR2 = GetIntake(userCollegeID, AYY2, item.specializationId, item.shiftId, 3);
                item.ExambranchadmittedIntakeR3 = GetIntake(userCollegeID, AYY3, item.specializationId, item.shiftId, 3);
                item.ExambranchadmittedIntakeR4 = GetIntake(userCollegeID, AYY4, item.specializationId, item.shiftId, 3);
                item.ExambranchadmittedIntakeR5 = GetIntake(userCollegeID, AYY5, item.specializationId, item.shiftId, 3);

                item.ExambranchadmittedIntakeL1 = GetIntake(userCollegeID, AYY1, item.specializationId, item.shiftId, 4);
                item.ExambranchadmittedIntakeL2 = GetIntake(userCollegeID, AYY2, item.specializationId, item.shiftId, 4);
                item.ExambranchadmittedIntakeL3 = GetIntake(userCollegeID, AYY3, item.specializationId, item.shiftId, 4);
                item.ExambranchadmittedIntakeL4 = GetIntake(userCollegeID, AYY4, item.specializationId, item.shiftId, 4);
                item.ExambranchadmittedIntakeL5 = GetIntake(userCollegeID, AYY5, item.specializationId, item.shiftId, 4);

                item.isintakeediable = true;
                if (item.Degree == "B.Tech")
                {
                    int senondyearpercentage = 0;
                    int thirdyearpercentage = 0;
                    int fourthyearpercentage = 0;
                    if (item.approvedIntake1 != 0)
                    {
                        senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(item.ExambranchadmittedIntakeR1) / Convert.ToDecimal(item.approvedIntake1)) * 100));
                    }
                    if (item.approvedIntake2 != 0)
                    {
                        thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(item.ExambranchadmittedIntakeR2) / Convert.ToDecimal(item.approvedIntake2)) * 100));
                    }
                    if (item.approvedIntake3 != 0)
                    {
                        fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(item.ExambranchadmittedIntakeR3) / Convert.ToDecimal(item.approvedIntake3)) * 100));
                    }

                    if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                    {
                        item.ispercentage = true;
                        item.isintakeediable = false;
                        //studentcount
                        if ((item.ExambranchadmittedIntakeR1 >= studentcount || item.ExambranchadmittedIntakeR2 >= studentcount || item.ExambranchadmittedIntakeR3 >= studentcount) && item.ProposedIntake != 0)
                        {
                            item.ispercentage = false;
                            item.isintakeediable = true;
                            //intakedetails.ReducedInatke = 60;
                            //if (intakedetails.approvedIntake1 != 60)
                            //{
                            //    intakedetails.Note = "(Reduced from Proposed Intake of <b>";
                            //    intakedetails.Note += intakedetails.approvedIntake1;
                            //    intakedetails.Note += "</b> as per 25% Clause)";
                            //    intakedetails.approvedIntake1 = 60;
                            //}
                        }
                    }
                    //int SanctionIntakeHigest = Max(item.admittedIntake1, item.admittedIntake2, item.admittedIntake3);
                    //SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                    //if (SanctionIntakeHigest >= item.ProposedIntake)
                    //{
                    //    item.isintakeediable = true;
                    //}
                    //else
                    //{
                    //    item.isintakeediable = false;
                    //}
                }

                intakelists.Add(item);
            }

            if (intakelists.Count > 0)
            {
                intakedetailsList.FirstOrDefault().CollegeIntakeExistings = intakelists.Where(i => i.shiftId == 1 || i.shiftId == 2).OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            }
            else
            {
                intakedetailsList.FirstOrDefault().CollegeIntakeExistings = new List<CollegeIntakeExisting>();
            }

            #endregion
            return View(intakedetailsList.Where(i => i.ispercentage == false).ToList());
        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult AppealAddEditCollegeIntake(int? id, int collegeId)
        {
            CollegeIntakeExisting collegeIntakeExisting = new CollegeIntakeExisting();
            int userCollegeID = collegeId;
            if (id != null && userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_intake_existing.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            ViewBag.IsUpdate = true;
            int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
            var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
            collegeIntakeExisting.collegeId = userCollegeID;
            collegeIntakeExisting.AICTEApprovalLettr = AICTEApprovalLettr;

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            ViewBag.AcademicYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));
            if (id != null)
            {
                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.id == id).ToList();

                foreach (var item in intake)
                {
                    collegeIntakeExisting.id = item.id;
                    collegeIntakeExisting.collegeId = item.collegeId;
                    collegeIntakeExisting.academicYearId = item.academicYearId;
                    collegeIntakeExisting.shiftId = item.shiftId;
                    collegeIntakeExisting.isActive = item.isActive;
                    collegeIntakeExisting.nbaFrom = item.nbaFrom;
                    collegeIntakeExisting.nbaTo = item.nbaTo;
                    collegeIntakeExisting.specializationId = item.specializationId;
                    collegeIntakeExisting.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    collegeIntakeExisting.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    collegeIntakeExisting.Department = db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    collegeIntakeExisting.degreeID = db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    collegeIntakeExisting.Degree = db.jntuh_degree.Where(d => d.id == collegeIntakeExisting.degreeID).Select(d => d.degree).FirstOrDefault();
                    collegeIntakeExisting.shiftId = item.shiftId;
                    collegeIntakeExisting.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                }

                if (collegeIntakeExisting.nbaFrom != null)
                    collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaFrom.ToString());
                if (collegeIntakeExisting.nbaTo != null)
                    collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaTo.ToString());

                //FLAG : 1 - Approved, 0 - Admitted
                jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                                                          .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == collegeIntakeExisting.specializationId && e.shiftId == collegeIntakeExisting.shiftId)
                                                          .Select(e => e)
                                                          .FirstOrDefault();
                if (details != null)
                {
                    collegeIntakeExisting.ApprovedIntake = details.approvedIntake;
                    collegeIntakeExisting.letterPath = details.approvalLetter;
                    collegeIntakeExisting.ProposedIntake = details.proposedIntake;
                    collegeIntakeExisting.courseStatus = details.courseStatus;
                }

                collegeIntakeExisting.approvedIntake1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);
                collegeIntakeExisting.AICTEapprovedIntake1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.ExambranchadmittedIntakeR1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntakeL1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);

                collegeIntakeExisting.approvedIntake2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);
                collegeIntakeExisting.AICTEapprovedIntake2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.ExambranchadmittedIntakeR2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntakeL2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);

                collegeIntakeExisting.approvedIntake3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);
                collegeIntakeExisting.AICTEapprovedIntake3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.ExambranchadmittedIntakeR3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntakeL3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);

                collegeIntakeExisting.approvedIntake4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);
                collegeIntakeExisting.AICTEapprovedIntake4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.ExambranchadmittedIntakeR4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntakeL4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);

                collegeIntakeExisting.approvedIntake5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);
                collegeIntakeExisting.AICTEapprovedIntake5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.ExambranchadmittedIntakeR5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntakeL5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);


                //collegeIntakeExisting.approvedIntake1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                //collegeIntakeExisting.admittedIntake1 = GetIntake(userCollegeID, AY1, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                //collegeIntakeExisting.approvedIntake2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                //collegeIntakeExisting.admittedIntake2 = GetIntake(userCollegeID, AY2, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                //collegeIntakeExisting.approvedIntake3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                //collegeIntakeExisting.admittedIntake3 = GetIntake(userCollegeID, AY3, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                //collegeIntakeExisting.approvedIntake4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                //collegeIntakeExisting.admittedIntake4 = GetIntake(userCollegeID, AY4, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                //collegeIntakeExisting.approvedIntake5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                //collegeIntakeExisting.admittedIntake5 = GetIntake(userCollegeID, AY5, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

            }
            else
            {
                ViewBag.IsUpdate = false;
            }

            var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).ToList();
            ViewBag.Degree = degrees.OrderBy(d => d.degree);
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.Count = degrees.Count();
            return PartialView("_AppealAddEditCollegeIntake", collegeIntakeExisting);

        }

        [Authorize(Roles = "Admin,College")]
        [HttpPost]
        public ActionResult AppealAddEditCollegeIntake(CollegeIntakeExisting collegeIntakeExisting, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = collegeIntakeExisting.collegeId;
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            if (collegeIntakeExisting.nbaFromDate != null)
                collegeIntakeExisting.nbaFrom = Convert.ToDateTime(Utilities.DDMMYY2MMDDYY(collegeIntakeExisting.nbaFromDate));
            if (collegeIntakeExisting.nbaToDate != null)
                collegeIntakeExisting.nbaTo = Convert.ToDateTime(Utilities.DDMMYY2MMDDYY(collegeIntakeExisting.nbaToDate));
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();
            if (ModelState.IsValid)
            {
                collegeIntakeExisting.collegeId = userCollegeID;
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                int presentAY = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                string nbafilename = string.Empty;
                var AppealAICTEApprovalReportspath = "~/Content/Upload/OnlineAppealDocuments/Intake/AppealAICTEApprovalReports";
                if (collegeIntakeExisting.AppealApprovalLetter != null)
                {
                    if (!Directory.Exists(Server.MapPath(AppealAICTEApprovalReportspath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(AppealAICTEApprovalReportspath));
                    }

                    var ext = Path.GetExtension(collegeIntakeExisting.AppealApprovalLetter.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        var fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        collegeIntakeExisting.AppealApprovalLetter.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(AppealAICTEApprovalReportspath),
                            fileName, ext));
                        nbafilename = string.Format("{0}{1}", fileName, ext);
                    }
                }

                for (int i = -1; i < 5; i++)
                {
                    int? approved = 0;
                    int admitted = 0;
                    int academicYear = 0;

                    int? proposed = null;
                    string letterPath = null;

                    if (i == -1)
                    {
                        approved = collegeIntakeExisting.ApprovedIntake != null ? collegeIntakeExisting.ApprovedIntake : 0;
                        admitted = 0;
                        academicYear = presentAY + 1;
                        //academicYear = presentAY + 1;

                        letterPath = collegeIntakeExisting.letterPath;
                        proposed = collegeIntakeExisting.ProposedIntake;
                    }
                    if (i == 0)
                    {
                        approved = collegeIntakeExisting.approvedIntake1;
                        admitted = collegeIntakeExisting.admittedIntake1;
                        academicYear = presentAY - i;
                        //academicYear = presentAY + 1;
                    }
                    if (i == 1)
                    {
                        approved = collegeIntakeExisting.approvedIntake2;
                        admitted = collegeIntakeExisting.admittedIntake2;
                        academicYear = presentAY - i;
                        //academicYear = presentAY;
                    }
                    if (i == 2)
                    {
                        approved = collegeIntakeExisting.approvedIntake3;
                        admitted = collegeIntakeExisting.admittedIntake3;
                        academicYear = presentAY - i;
                        //academicYear = presentAY - 1;
                    }
                    if (i == 3)
                    {
                        approved = collegeIntakeExisting.approvedIntake4;
                        admitted = collegeIntakeExisting.admittedIntake4;
                        academicYear = presentAY - i;
                        //academicYear = presentAY - 2;
                    }
                    if (i == 4)
                    {
                        approved = collegeIntakeExisting.approvedIntake5;
                        admitted = collegeIntakeExisting.admittedIntake5;
                        academicYear = presentAY - i;
                        //academicYear = presentAY - 3;
                    }

                    var jntuh_college_intake_existing = new jntuh_appeal_college_intake_existing();
                    jntuh_college_intake_existing.academicYearId = db.jntuh_academic_year.Where(a => a.actualYear == academicYear).Select(a => a.id).FirstOrDefault();

                    var existingId = db.jntuh_appeal_college_intake_existing.Where(p => p.specializationId == collegeIntakeExisting.specializationId
                                                                                && p.shiftId == collegeIntakeExisting.shiftId
                                                                                && p.collegeId == collegeIntakeExisting.collegeId
                                                                                && p.academicYearId == jntuh_college_intake_existing.academicYearId).Select(p => p.id).FirstOrDefault();
                    int createdByu = Convert.ToInt32(db.jntuh_appeal_college_intake_existing.Where(a => a.collegeId == userCollegeID && a.id == existingId).Select(a => a.createdBy).FirstOrDefault());
                    DateTime createdonu = Convert.ToDateTime(db.jntuh_appeal_college_intake_existing.Where(a => a.collegeId == userCollegeID && a.id == existingId).Select(a => a.createdOn).FirstOrDefault());

                    if ((approved > 0 && i != -1) || (i != -1 && admitted > 0 && existingId == 0) || (existingId > 0) || (i == -1))
                    {
                        jntuh_college_intake_existing.id = collegeIntakeExisting.id;
                        jntuh_college_intake_existing.collegeId = collegeIntakeExisting.collegeId;
                        jntuh_college_intake_existing.academicYearId = jntuh_academic_year.Where(a => a.actualYear == academicYear).Select(a => a.id).FirstOrDefault();
                        jntuh_college_intake_existing.specializationId = collegeIntakeExisting.specializationId;
                        jntuh_college_intake_existing.shiftId = collegeIntakeExisting.shiftId;
                        jntuh_college_intake_existing.approvedIntake = (int)approved;
                        jntuh_college_intake_existing.admittedIntake = admitted;
                        jntuh_college_intake_existing.approvalLetter = nbafilename; //new
                        jntuh_college_intake_existing.proposedIntake = proposed;  //new
                        jntuh_college_intake_existing.nbaFrom = collegeIntakeExisting.nbaFrom;
                        jntuh_college_intake_existing.nbaTo = collegeIntakeExisting.nbaTo;
                        jntuh_college_intake_existing.isActive = true;
                        jntuh_college_intake_existing.courseStatus = collegeIntakeExisting.courseStatus ?? "";


                        if (existingId == 0)
                        {
                            jntuh_college_intake_existing.createdBy = userID;
                            jntuh_college_intake_existing.createdOn = DateTime.Now;
                            db.jntuh_appeal_college_intake_existing.Add(jntuh_college_intake_existing);
                            db.SaveChanges();
                        }
                        else
                        {
                            jntuh_college_intake_existing.id = existingId;
                            jntuh_college_intake_existing.createdBy = createdByu;
                            jntuh_college_intake_existing.createdOn = createdonu;
                            jntuh_college_intake_existing.updatedBy = userID;
                            jntuh_college_intake_existing.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_intake_existing).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException dbEx)
                            {
                                foreach (var validationErrors in dbEx.EntityValidationErrors)
                                {
                                    foreach (var validationError in validationErrors.ValidationErrors)
                                    {
                                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                                validationError.PropertyName,
                                                                validationError.ErrorMessage);
                                    }
                                }
                            }
                        }

                    }
                }

                if (cmd == "Add")
                {
                    TempData["Success"] = "Intake Added successfully.";
                }
                else
                {
                    TempData["Success"] = "Intake Updated successfully.";
                }

                return RedirectToAction("AppealExistingAndProposedIntake");
            }
            else
            {
                return RedirectToAction("AppealExistingAndProposedIntake");
            }

        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult AppealDeleteCollegeIntake(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_intake_existing.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
            }
            int specid = db.jntuh_college_intake_existing.Where(p => p.id == id).Select(p => p.specializationId).FirstOrDefault();
            int shiftid = db.jntuh_college_intake_existing.Where(p => p.id == id).Select(p => p.shiftId).FirstOrDefault();
            List<jntuh_college_intake_existing> jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(p => p.specializationId == specid && p.shiftId == shiftid && p.collegeId == userCollegeID).ToList();
            foreach (var item in jntuh_college_intake_existing)
            {
                db.jntuh_college_intake_existing.Remove(item);
                //db.SaveChanges();
                TempData["Success"] = "College Intake Deleted successfully";
            }

            return RedirectToAction("CollegeFacultyWithIntake");
        }

        #region Get Intake Methods

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            //Changes in place of 10 to 11 by Narayana on 16-03-2018
            //approved
            if (flag == 1 && academicYearId != 11)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.approvedIntake).FirstOrDefault();
                //New Code Writtenby Narayana
            }
            else if (flag == 1 && academicYearId == 11)
            {
                var inta = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).FirstOrDefault();
                if (inta != null && inta.proposedIntake != null)
                {
                    intake = (int)inta.proposedIntake;
                }
            }
            else if (flag == 2)//AICTE Intake
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.aicteApprovedIntake).FirstOrDefault() == null ? 0 : db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.aicteApprovedIntake).FirstOrDefault();
            }
            else if (flag == 0)//admitted
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.admittedIntake).FirstOrDefault();
            }
            else if (flag == 3)//ExamsBranchAddmitted-R
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.admittedIntakeasperExambranch_R).FirstOrDefault();
            }
            else if (flag == 4)//ExamsBranchAddmitted-L
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.admittedIntakeasperExambranch_L).FirstOrDefault();
            }
            return intake;
        }

        private int GetIntakeBtech(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;

            //Degree B.Tech  
            if (DegreeId == 4)
            {
                //approved-0,admitted-1,Aicte-2,Exam Branch admitted-3
                if (flag == 1 && (academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 11)//Proposed Intake
                {
                    //New Code
                    var appealinta = db.jntuh_appeal_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (appealinta != null)
                    {
                        intake = Convert.ToInt32(appealinta.proposedIntake);
                    }
                    else
                    {
                        var inta =
                            db.jntuh_college_intake_existing.FirstOrDefault(
                                i =>
                                    i.collegeId == collegeId && i.academicYearId == academicYearId &&
                                    i.specializationId == specializationId && i.shiftId == shiftId);
                        if (inta != null)
                        {
                            intake = Convert.ToInt32(inta.proposedIntake);
                        }
                    }

                }
                else if (flag == 2)
                {
                    var intakes = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i).FirstOrDefault();
                    if (intakes != null)
                        intake = Convert.ToInt32(intakes.aicteApprovedIntake);
                    else
                        intake = 0;
                }
                else if (flag == 3)
                {
                    var intakes = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i).FirstOrDefault();
                    if (intakes != null)
                        intake = Convert.ToInt32(intakes.admittedIntakeasperExambranch_R);
                    else
                        intake = 0;
                }
                else  //approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            else
            {
                //approved-0,admitted-1,Aicte-2,Exam Branch admitted-3
                if (flag == 1 && (academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 11)//Proposed Intake
                {
                    //New Code
                    var appealinta = db.jntuh_appeal_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (appealinta != null)
                    {
                        intake = Convert.ToInt32(appealinta.proposedIntake);
                    }
                    else
                    {
                        var inta =
                            db.jntuh_college_intake_existing.FirstOrDefault(
                                i =>
                                    i.collegeId == collegeId && i.academicYearId == academicYearId &&
                                    i.specializationId == specializationId && i.shiftId == shiftId);
                        if (inta != null)
                        {
                            intake = Convert.ToInt32(inta.proposedIntake);
                        }
                    }

                }
                else if (flag == 2)
                {
                    var intakes = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i).FirstOrDefault();
                    if (intakes != null)
                        intake = Convert.ToInt32(intakes.aicteApprovedIntake);
                    else
                        intake = 0;
                }
                else if (flag == 3)
                {
                    var intakes = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i).FirstOrDefault();
                    if (intakes != null)
                        intake = Convert.ToInt32(intakes.admittedIntakeasperExambranch_R);
                    else
                        intake = 0;
                }
                else  //approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            return intake;
        }

        private bool GetAcademicYear(int collegeId, int academicYearId, int specializationId, int shiftId, int DegreeId)
        {
            var firstOrDefault = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.courseAffiliatedStatus).FirstOrDefault();
            return firstOrDefault ?? false;
        }

        private int GetBtechAdmittedIntake(int Intake)
        {
            int BtechAdmittedIntake = 0;
            if (Intake >= 0 && Intake <= 60)
            {
                BtechAdmittedIntake = 60;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                BtechAdmittedIntake = 120;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                BtechAdmittedIntake = 180;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                BtechAdmittedIntake = 240;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                BtechAdmittedIntake = 300;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                BtechAdmittedIntake = 360;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                BtechAdmittedIntake = 420;
            }
            return BtechAdmittedIntake;
        }

        public int IntakeWisePhdForBtech(int Intake)
        {
            int Phdcount = 0;
            if (Intake > 0 && Intake <= 60)
            {
                Phdcount = 0;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                Phdcount = 1;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                Phdcount = 2;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                Phdcount = 3;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                Phdcount = 4;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                Phdcount = 5;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                Phdcount = 6;
            }
            return Phdcount;
        }

        public int IntakeWisePhdForMtech(int Intake, int shiftid)
        {
            int Phdcount = 0;
            if (shiftid == 1)
            {
                if (Intake > 0 && Intake <= 30)
                {
                    Phdcount = 2;
                }
                else if (Intake > 30 && Intake <= 60)
                {
                    Phdcount = 4;
                }
            }
            else
            {
                if (Intake > 0 && Intake <= 30)
                {
                    Phdcount = 1;
                }
                else if (Intake > 30 && Intake <= 60)
                {
                    Phdcount = 2;
                }
            }

            return Phdcount;
        }

        public int GetSectionBasedonIntake(int Intake)
        {
            int Section = 0;

            if (Intake > 0 && Intake <= 60)
            {
                Section = 1;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                Section = 2;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                Section = 3;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                Section = 4;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                Section = 5;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                Section = 6;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                Section = 7;
            }
            return Section;
        }

        public int IntakeWisePhdForMBAandMCA(int Intake)
        {
            int Phdcount = 0;
            if (Intake > 0 && Intake <= 60)
            {
                Phdcount = 1;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                Phdcount = 2;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                Phdcount = 3;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                Phdcount = 4;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                Phdcount = 5;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                Phdcount = 6;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                Phdcount = 7;
            }

            return Phdcount;
        }

        private int GetPharmacyIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            if (flag == 0) //Proposed
            {
                var inta = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).FirstOrDefault();
                if (inta != null && inta.proposedIntake != null)
                {
                    intake = (int)inta.proposedIntake;
                }
            }
            else if (flag == 1)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.aicteApprovedIntake).FirstOrDefault() == null ? 0 : db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
            }
            else if (flag == 2)//AICTE Intake
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.aicteApprovedIntake).FirstOrDefault() == null ? 0 : db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            }
            return intake;
        }

        private int Max(int AdmittedIntake2, int AdmittedIntake3, int AdmittedIntake4)
        {
            return Math.Max(AdmittedIntake2, Math.Max(AdmittedIntake3, AdmittedIntake4));
        }
        #endregion

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult ALLCollegeFacultyWithIntakeFaculty()
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var collegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    //return RedirectToAction("CollegeDashboard", "Dashboard");
                    return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            var SubmittedCollegeIds = db.jntuh_college_edit_status.Where(a => a.IsCollegeEditable == false).Select(a => a.collegeId).ToList();
            int[] integreatedIds = { 39, 42, 43, 75, 140, 180, 194, 217, 223, 266, 332, 364, 9, 35, 50, 273, 369 };

            if (integreatedIds.Contains(collegeId))
            {
                return RedirectToAction("IntegratedCollegesFacultyWithIntake", "CollegeAppeal");
            }

            var pharmacyids = db.jntuh_college.Where(z => SubmittedCollegeIds.Contains(z.id) && z.collegeTypeID == 2).Select(q => q.id).ToList();

            if (pharmacyids.Contains(collegeId))
            {
                return RedirectToAction("CollegeFacultyWithIntakeFacultyPharmacyNew", "CollegeAppeal");
            }

            return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal");
        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult IntegratedCollegesFacultyWithIntake()
        {
            return View();
        }


        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult AppealCollegeFacultyWithIntake(string type)
        {
            #region new code
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeid = collegeId;
            #region
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }

            // Principal Details
            string strPrincipalRegno = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf.RegistrationNumber).FirstOrDefault();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            //Reg nos related online facultyIds
            var registeredPrincipal = db.jntuh_registered_faculty.Where(rf => strPrincipalRegno == rf.RegistrationNumber && (rf.collegeId == collegeId)).ToList();


            var jntuh_Principals_registered = registeredPrincipal.Where(rf => rf.DepartmentId != null && rf.DeactivationReason == "")
                                                 .Select(rf => new
                                                 {
                                                     RegistrationNumber = rf.RegistrationNumber,
                                                     Department = rf.jntuh_department.departmentName,
                                                     HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                     IsApproved = rf.isApproved,
                                                     PanNumber = rf.PANNumber,
                                                     AadhaarNumber = rf.AadhaarNumber
                                                 }).ToList();
            ViewBag.PrincipalRegno = strPrincipalRegno;
            if (jntuh_Principals_registered.Count > 0)
                ViewBag.PrincipalDeficiency = "NO";
            else
                ViewBag.PrincipalDeficiency = "YES";
            #endregion


            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
             {
                 collegeId = c.id,
                 collegeName = c.collegeCode + "-" + c.collegeName
             }).OrderBy(c => c.collegeName).ToList();

            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            if (collegeId != null)
            {
                int userCollegeID = (int)collegeId;
                var jntuh_specialization = db.jntuh_specialization.ToList();
                int[] collegeIDs = null;
                int facultystudentRatio = 0;
                decimal facultyRatio = 0m;
                if (collegeId != 0)
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                }
                else
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
                }
                var jntuh_departments = db.jntuh_department.ToList();
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
                var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
                var jntuh_degree = db.jntuh_degree.ToList();

                int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
                var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();

                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int studentcount = Convert.ToInt32(ConfigurationManager.AppSettings["studentcount"]);
                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();
                foreach (var item in intake)
                {
                    CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.isActive = item.isActive;
                    newIntake.nbaFrom = item.nbaFrom;
                    newIntake.nbaTo = item.nbaTo;
                    newIntake.specializationId = item.specializationId;
                    newIntake.Specialization = item.jntuh_specialization.specializationName;
                    newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                    newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                    newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                    newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                    newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
                    collegeIntakeExisting.Add(newIntake);
                }
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

                //New 
                var DeptNameBasedOnSpecialization = (from a in db.jntuh_department
                                                     join b in db.jntuh_specialization on a.id equals b.departmentId
                                                     select new
                                                     {
                                                         DeptId = a.id,
                                                         DeptName = a.departmentName,
                                                         Specid = b.id
                                                     }).ToList();




                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                //Reg nos related online facultyIds
                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList()
                    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& rf.RegistrationNumber != principalRegno
                //&& (rf.collegeId == null || rf.collegeId == collegeId)
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var regfacultywithoutdepts = registeredFaculty.Where(r => r.DepartmentId == null).Select(i => i.type);

                var jntuh_registered_faculty1 =
                   registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                       && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                                                       {
                                                           //Departmentid = rf.DepartmentId,
                                                           RegistrationNumber = rf.RegistrationNumber.Trim(),
                                                           // Department = rf.jntuh_department.departmentName,
                                                           DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                                                           SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                                                           NotconsideredPHD = rf.NotconsideredPHD,
                                                           // HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                                                           HighestDegreeID = rf.NotconsideredPHD == true ? rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6).Select(e => e.educationId).Max() : 0 : rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                                                           IsApproved = rf.isApproved,
                                                           Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
                                                           PanNumber = rf.PANNumber,
                                                           AadhaarNumber = rf.AadhaarNumber,
                                                           NoForm16 = rf.NoForm16,
                                                           TotalExperience = rf.TotalExperience

                                                       }).ToList();

                //var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                //                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes") && rf.InvalidAadhaar != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27))
                //                                        .Select(rf => new
                //                                        {
                //                                            RegistrationNumber = rf.RegistrationNumber,
                //                                            DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                //                                            SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                //                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                //                                            IsApproved = rf.isApproved,
                //                                            PanNumber = rf.PANNumber,
                //                                            AadhaarNumber = rf.AadhaarNumber,
                //                                            TotalExperience = rf.TotalExperience
                //                                        }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    rf.SpecializationId,
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    TotalExperience = rf.TotalExperience
                }).Where(e => e.Department != null).ToList();


                var pharmacydeptids = new[] { 26, 27, 36, 39 };
                collegeIntakeExisting = collegeIntakeExisting.Where(i => !pharmacydeptids.Contains(i.DepartmentID)).ToList();
                foreach (var item in collegeIntakeExisting)
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty = 0;
                    int pgFaculty = 0;
                    int ugFaculty = 0;
                    int SpecializationphdFaculty = 0;
                    int SpecializationpgFaculty = 0;
                    intakedetails.isintakeeditable = false;
                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.DegreeId = item.degreeID;
                    intakedetails.shiftId = item.shiftId;

                    intakedetails.ProposedIntake = GetIntakeBtech(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake1 = GetIntakeBtech(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake2 = GetIntakeBtech(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake3 = GetIntakeBtech(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake4 = GetIntakeBtech(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake5 = GetIntakeBtech(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);

                    intakedetails.AICTESanctionIntake1 = GetIntakeBtech(item.collegeId, AY1, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake2 = GetIntakeBtech(item.collegeId, AY2, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake3 = GetIntakeBtech(item.collegeId, AY3, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake4 = GetIntakeBtech(item.collegeId, AY4, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake5 = GetIntakeBtech(item.collegeId, AY5, item.specializationId, item.shiftId, 2, item.degreeID);

                    intakedetails.ExambranchIntake_R1 = GetIntakeBtech(item.collegeId, AY1, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R2 = GetIntakeBtech(item.collegeId, AY2, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R3 = GetIntakeBtech(item.collegeId, AY3, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R4 = GetIntakeBtech(item.collegeId, AY4, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R5 = GetIntakeBtech(item.collegeId, AY5, item.specializationId, item.shiftId, 3, item.degreeID);


                    intakedetails.AffiliationStatus2 = GetAcademicYear(item.collegeId, AY1, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus3 = GetAcademicYear(item.collegeId, AY2, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus4 = GetAcademicYear(item.collegeId, AY3, item.specializationId, item.shiftId, item.degreeID);

                    if (item.degreeID == 4)
                    {
                        intakedetails.SanctionIntake1 = GetIntakeBtech(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake2 = GetIntakeBtech(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake3 = GetIntakeBtech(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake4 = GetIntakeBtech(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake5 = GetIntakeBtech(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);
                    }



                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    if (item.Degree == "B.Tech")
                    {

                        //Take Higest of 3 Years Of Admitated Intake
                        //approvedIntake1 means Proposed Intake of Present Year
                        //int SanctionIntakeHigest = Max(intakedetails.approvedIntake2, intakedetails.approvedIntake3, intakedetails.approvedIntake4);
                        //SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                        if (CollegeAffiliationStatus == "Yes")
                        {
                            intakedetails.ispercentage = false;
                        }
                        else
                        {
                            int senondyearpercentage = 0;
                            int thirdyearpercentage = 0;
                            int fourthyearpercentage = 0;
                            if (intakedetails.SanctionIntake2 != 0)
                            {
                                senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R2) / Convert.ToDecimal(intakedetails.SanctionIntake2)) * 100));
                            }
                            if (intakedetails.SanctionIntake3 != 0)
                            {
                                thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R3) / Convert.ToDecimal(intakedetails.SanctionIntake3)) * 100));
                            }
                            if (intakedetails.SanctionIntake4 != 0)
                            {
                                fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R4) / Convert.ToDecimal(intakedetails.SanctionIntake4)) * 100));
                            }

                            if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                            {
                                intakedetails.ispercentage = true;
                                intakedetails.isintakeeditable = false;
                                if ((intakedetails.ExambranchIntake_R2 >= studentcount || intakedetails.ExambranchIntake_R3 >= studentcount || intakedetails.ExambranchIntake_R4 >= studentcount) && intakedetails.ProposedIntake != 0)
                                {
                                    intakedetails.ispercentage = false;
                                    intakedetails.isintakeeditable = true;
                                    //intakedetails = false;
                                    //intakedetails.ReducedInatke = 60;
                                    //if (intakedetails.approvedIntake1 != 60)
                                    //{
                                    //    intakedetails.Note = "(Reduced from Proposed Intake of <b>";
                                    //    intakedetails.Note += intakedetails.approvedIntake1;
                                    //    intakedetails.Note += "</b> as per 25% Clause)";
                                    //    intakedetails.approvedIntake1 = 60;
                                    //}
                                }
                            }
                        }

                        if (intakedetails.ispercentage == false)
                        {
                            //if (intakedetails.isintakeeditable == true)
                            //{
                            //    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                            //    intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                            //    intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            //}
                            //else if (SanctionIntakeHigest >= intakedetails.approvedIntake1)
                            //{
                            //    //New Code 
                            //    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.approvedIntake2);
                            //    intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.approvedIntake3);
                            //    intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.approvedIntake4);



                            //}
                            //else
                            //{
                            //    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                            //    intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                            //    intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            //}
                            intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) + GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                                                       GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            //int totalinatakeroundoff = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) + GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                            //                           GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);


                            intakedetails.totalAdmittedIntake = (intakedetails.admittedIntake2) + (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4);
                        }



                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        if (item.Degree == "M.Tech")
                        {
                            if (item.Degree == "M.Tech" && item.shiftId == 1)
                            {
                                //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                                intakedetails.totalIntake = (intakedetails.ProposedIntake) + (intakedetails.AICTESanctionIntake2);
                                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                               Convert.ToDecimal(facultystudentRatio);
                                int fratio = (int)facultyRatio;
                                if (fratio < 3)
                                {
                                    fratio = 3;
                                    facultyRatio = Convert.ToDecimal(fratio);
                                }
                            }
                            if (item.Degree == "M.Tech" && item.shiftId == 2)
                            {
                                intakedetails.totalIntake = (intakedetails.ProposedIntake) + (intakedetails.ProposedIntake);
                                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                               Convert.ToDecimal(facultystudentRatio);
                                int fratio = (int)facultyRatio;
                                if (fratio < 3)
                                {
                                    fratio = 3;
                                    facultyRatio = Convert.ToDecimal(fratio);
                                }
                                facultyRatio = facultyRatio / 2;
                            }
                        }
                        else
                        {
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            intakedetails.totalIntake = (intakedetails.ProposedIntake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                        }


                    }
                    else if (item.Degree == "MCA")
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                        //                            (intakedetails.approvedIntake3);
                        intakedetails.totalIntake = (intakedetails.ProposedIntake) + (intakedetails.AICTESanctionIntake2) +
                                                    (intakedetails.AICTESanctionIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 15;

                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                    }
                    else //MAM MTM Pharm.D Pharm.D PB
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                        //                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                        //                            (intakedetails.approvedIntake5);
                        intakedetails.totalIntake = (intakedetails.ProposedIntake) + (intakedetails.AICTESanctionIntake2) +
                                                    (intakedetails.AICTESanctionIntake3) + (intakedetails.AICTESanctionIntake4) +
                                                    (intakedetails.AICTESanctionIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                    }
                    intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

                    string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
                    if (strdegreetype == "UG")
                    {
                        if (item.Degree == "B.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D PB" && f.Recruitedfor == "UG");
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                        }
                    }
                    if (strdegreetype == "PG")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                            intakedetails.specializationWiseFacultyPHDFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        }
                    }

                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);
                    }
                    if (item.Degree == "B.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);

                    }
                    if (item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);

                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);

                        intakedetails.Department = "Pharm.D PB";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
                        if (item.Degree == "B.Tech")
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                        else
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                        SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));

                    }


                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                    intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    //=============//


                    intakedetailsList.Add(intakedetails);
                }
                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
                int[] OthersSpecIds = new int[] { 155, 156, 157, 158 };
                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
                int btechdegreecount = intakedetailsList.Count(d => d.Degree == "B.Tech");

                if (btechdegreecount != 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                        var degreeId = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.degreeId).FirstOrDefault();
                        var deparmentid = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.departmentId).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Count(f => f.Department == department && f.HighestDegree == "UG");
                        int pgFaculty = jntuh_registered_faculty.Count(f => (f.HighestDegree == "PG" || f.HighestDegree == "M.Phil / Other PG Degree") && f.Department == department);
                        int phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);
                        intakedetailsList.Add(new CollegeFacultyWithIntakeReport
                        {
                            collegeId = (int)collegeId,
                            Degree = "B.Tech",
                            Department = department,
                            Specialization = department,
                            DegreeId = degreeId,
                            ugFaculty = ugFaculty,
                            pgFaculty = pgFaculty,
                            phdFaculty = phdFaculty,
                            totalFaculty = ugFaculty + pgFaculty + phdFaculty,
                            specializationId = speId,
                            shiftId = 1,
                            DepartmentID = deparmentid,
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname),
                            ProposedIntake = 1
                        });

                    }
                }

                List<CollegeFacultyWithIntakeReport> facultyCounts = intakedetailsList.Where(i => i.shiftId == 1 || i.shiftId == 2).ToList();
                List<CollegeFacultyWithIntakeReport> facultyCountsmetechsecond = facultyCounts.Where(c => c.shiftId == 2).Select(e => e).ToList();
                foreach (var item in facultyCountsmetechsecond)
                {
                    int id =
                        facultyCounts.Where(
                            s => s.specializationId == item.specializationId && s.shiftId == 1 && s.Degree == "M.Tech" && s.ProposedIntake != 0)
                            .Select(s => s.shiftId)
                            .FirstOrDefault();
                    if (id == 0)
                    {
                        facultyCounts.Remove(item);
                    }
                }
                List<CollegeFacultyWithIntakeReport> facultyCountper = intakedetailsList.Where(c => c.collegeId == userCollegeID && ((c.ispercentage == true && c.ProposedIntake != 0 && c.Degree == "B.Tech") || c.ProposedIntake == 0 && c.Degree == "B.Tech")).Select(e => e).ToList();
                foreach (var itemmtech in facultyCountper)
                {
                    if (itemmtech.collegeId == 72 && itemmtech.Department == "IT")
                    {
                    }
                    else if (itemmtech.collegeId == 130 && itemmtech.Department == "IT")
                    {

                    }
                    else
                    {
                        List<CollegeFacultyWithIntakeReport> notshownmtech = facultyCounts.Where(
                           s => s.Department == itemmtech.Department && s.Degree == "M.Tech" && s.ProposedIntake != 0)
                           .Select(s => s)
                           .ToList();
                        if (notshownmtech.Count() != 0)
                        {
                            // facultyCounts.Remove(itemmtech);
                            foreach (var removemtech in notshownmtech)
                            {
                                facultyCounts.Remove(removemtech);
                            }

                        }
                    }
                }

                int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech" && !strOtherDepartments.Contains(d.Department)).Select(d => d.ProposedIntake).Sum();
                var degrees = db.jntuh_degree.ToList();
                var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 160);
                int remainingFaculty = 0;
                int remainingPHDFaculty = 0;
                decimal departmentWiseRequiredFaculty = 0;
                var distDeptcount = 1;
                var deptloop = 1;
                int HumantitiesminimamRequireMet = 0;
                string HumantitiesminimamRequireMetStatus = "Yes";

                // var TotalRequiredHumanitiesFacultyCount = Math.Ceiling((double)totalBtechFirstYearIntake / 20);
                // ViewBag.TotalRequiredHumanitiesFacultyCount = TotalRequiredHumanitiesFacultyCount;

                intakedetailsList = facultyCounts.Where(i => i.shiftId == 1 || i.shiftId == 2).ToList();


                foreach (var item in intakedetailsList)
                {

                    var SpecializationwisePHDFaculty = 0;
                    //if (item.Degree == "M.Tech" || item.Degree == "B.Tech")
                    //    SpecializationwisePHDFaculty = intakedetailsList.Where(D => D.Department == item.Department && D.Degree == "M.Tech" && D.shiftId == 1).Distinct().Count();
                    //else if (item.Degree == "MCA")
                    //    SpecializationwisePHDFaculty = intakedetailsList.Where(D => D.Department == item.Department && D.Degree == "MCA" && D.shiftId == 1).Distinct().Count();
                    //else if (item.Degree == "MBA")
                    //    SpecializationwisePHDFaculty = intakedetailsList.Where(D => D.Department == item.Department && D.Degree == "MBA" && D.shiftId == 1).Distinct().Count();
                    //SpecializationwisePHDFaculty = SpecializationwisePHDFaculty * 2;

                    if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA" || item.Degree == "5-Year MBA(Integrated)")
                    {

                        if (item.Degree == "M.Tech")
                        {
                            if (item.shiftId == 1)
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.ProposedIntake, item.shiftId);
                            }
                            if (item.shiftId == 2)
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.ProposedIntake, item.shiftId);
                            }
                        }
                        else if (item.Degree == "MCA")
                        {
                            SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.ProposedIntake);
                        }
                        else if (item.Degree == "MBA" || item.Degree == "5-Year MBA(Integrated)")
                        {
                            SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.ProposedIntake);
                        }


                    }
                    else if (item.Degree == "B.Tech")
                    {
                        SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.ProposedIntake);
                    }


                    distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

                    int indexnow = facultyCounts.IndexOf(item);

                    if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                    {
                        deptloop = 1;
                    }

                    departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                    string minimumRequirementMet = string.Empty;
                    string PhdminimumRequirementMet = string.Empty;
                    int facultyShortage = 0;
                    int adjustedFaculty = 0;
                    int adjustedPHDFaculty = 0;
                    int tFaculty = 0;
                    int othersRequiredfaculty = 0;

                    if (item.Department == "MBA" || item.Department == "MCA")
                        tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                    else
                        tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                    int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                    if (strOtherDepartments.Contains(item.Department))
                    {
                        if (OthersSpecIds.Contains(item.specializationId))
                        {

                            double rid = (double)(firstYearRequired / 2);
                            rFaculty = (int)(Math.Ceiling(rid));
                            // rFaculty = (int)firstYearRequired;
                            // othersRequiredfaculty = 1;
                        }
                        else
                        {
                            rFaculty = (int)firstYearRequired;
                        }
                    }

                    var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                    if (deptloop == 1)
                    {
                        if (rFaculty <= tFaculty)
                        {
                            minimumRequirementMet = "YES";
                            item.deficiency = false;
                            remainingFaculty = tFaculty - rFaculty;
                            adjustedFaculty = rFaculty;//tFaculty
                            item.BtechAdjustedFaculty = adjustedFaculty;
                        }
                        else
                        {
                            minimumRequirementMet = "NO";
                            item.deficiency = true;
                            adjustedFaculty = tFaculty;
                            remainingFaculty = 0;
                            //item.BtechAdjustedFaculty = adjustedFaculty;
                            //facultyShortage = rFaculty - tFaculty;
                        }

                        remainingPHDFaculty = item.phdFaculty;

                        //if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)//item.requiredFaculty
                        //{

                        //    if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                        //    {
                        //        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                        //        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                        //        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                        //        {
                        //            PhdminimumRequirementMet = "YES";
                        //            item.PHDdeficiency = false;

                        //        }
                        //        else
                        //        {
                        //            PhdminimumRequirementMet = "NO";
                        //            item.PHDdeficiency = true;
                        //        }

                        //    }
                        //    else
                        //    {
                        //        remainingPHDFaculty = remainingPHDFaculty;
                        //        adjustedPHDFaculty = remainingPHDFaculty;
                        //        if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                        //        {
                        //            PhdminimumRequirementMet = "YES";
                        //            item.PHDdeficiency = false;

                        //        }
                        //        else
                        //        {
                        //            PhdminimumRequirementMet = "NO";
                        //            item.PHDdeficiency = true;
                        //        }
                        //    }

                        //}
                        //else if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)//item.requiredFaculty
                        //{
                        //    //adjustedPHDFaculty = 1;
                        //    item.PHDdeficiency = true;
                        //    adjustedPHDFaculty = remainingPHDFaculty;
                        //    remainingPHDFaculty = remainingPHDFaculty - 1;
                        //    PhdminimumRequirementMet = "NO";
                        //    //adjustedPHDFaculty = remainingPHDFaculty;
                        //    //item.AvailablePHDFaculty = 2;
                        //    //intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = true);
                        //    //remainingPHDFaculty = remainingPHDFaculty - 1;
                        //}
                        if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                           item.SpecializationsphdFaculty > 0)
                        {
                            if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                            {
                                if (item.shiftId == 1)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    }
                                    else
                                    {
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    }
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                            }
                            else
                            {
                                remainingPHDFaculty = remainingPHDFaculty;
                                adjustedPHDFaculty = remainingPHDFaculty;

                                if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                    PhdminimumRequirementMet = "YES";
                                else
                                    PhdminimumRequirementMet = "NO";
                            }
                        }
                        else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                 item.SpecializationsphdFaculty > 0)
                        {
                            adjustedPHDFaculty = remainingPHDFaculty;
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                            PhdminimumRequirementMet = "NO";
                        }
                        else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty > 0)
                        {
                            //remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                            //adjustedPHDFaculty = SpecializationwisePHDFaculty;
                            if (remainingPHDFaculty >= SpecializationwisePHDFaculty)//item.SpecializationsphdFaculty
                            {
                                remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                adjustedPHDFaculty = SpecializationwisePHDFaculty; //item.SpecializationsphdFaculty;
                                PhdminimumRequirementMet = "YES";
                                item.PHDdeficiency = false;
                            }
                            else if (remainingPHDFaculty <= SpecializationwisePHDFaculty)//item.SpecializationsphdFaculty
                            {

                                adjustedPHDFaculty = remainingPHDFaculty;//SpecializationwisePHDFaculty;
                                remainingPHDFaculty = 0;//remainingPHDFaculty - SpecializationwisePHDFaculty;
                                // adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                PhdminimumRequirementMet = "NO";
                                item.PHDdeficiency = true;
                            }



                        }
                        else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                        {
                            // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            PhdminimumRequirementMet = "NO";
                            item.PHDdeficiency = true;

                        }
                        else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                        {
                            PhdminimumRequirementMet = "YES";
                            item.PHDdeficiency = false;
                        }
                        else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                        {
                            PhdminimumRequirementMet = "YES";
                            item.PHDdeficiency = false;
                        }
                        //Dual Degree Checking
                        else if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
                        {
                            if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                            {
                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                {
                                    PhdminimumRequirementMet = "YES";
                                    item.PHDdeficiency = false;
                                }
                                else
                                {
                                    PhdminimumRequirementMet = "NO";
                                    item.PHDdeficiency = true;
                                }

                            }
                            else
                            {

                                remainingPHDFaculty = remainingPHDFaculty;
                                adjustedPHDFaculty = remainingPHDFaculty;

                                if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                {
                                    PhdminimumRequirementMet = "YES";
                                    item.PHDdeficiency = false;
                                }
                                else
                                {
                                    PhdminimumRequirementMet = "NO";
                                    item.PHDdeficiency = true;
                                }

                            }

                        }
                        else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty >= 0)
                        {
                            adjustedPHDFaculty = remainingPHDFaculty;
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                            {
                                PhdminimumRequirementMet = "YES";
                                item.PHDdeficiency = false;
                            }
                            else
                            {
                                PhdminimumRequirementMet = "NO";
                                item.PHDdeficiency = true;
                            }
                            //PhdminimumRequirementMet = "NO";
                            //item.PHDdeficiency = true;
                        }
                        else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("Dual Degree")))
                        {
                            PhdminimumRequirementMet = "NO";
                            item.PHDdeficiency = true;
                        }


                    }
                    else
                    {
                        if (rFaculty <= remainingFaculty)
                        {
                            minimumRequirementMet = "YES";
                            item.deficiency = false;
                            if (degreeType.Equals("PG"))
                            {
                                if (item.shiftId == 1)
                                {
                                    if (item.specializationWiseFaculty >= rFaculty)
                                    {
                                        if (rFaculty <= item.specializationWiseFaculty)
                                        {
                                            remainingFaculty = remainingFaculty - rFaculty;
                                            adjustedFaculty = rFaculty;
                                        }

                                        else if (rFaculty >= item.specializationWiseFaculty)
                                        {
                                            remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                            adjustedFaculty = item.specializationWiseFaculty;
                                        }
                                    }
                                    else
                                    {
                                        minimumRequirementMet = "NO";
                                        item.deficiency = true;
                                        remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                        adjustedFaculty = item.specializationWiseFaculty;
                                    }
                                }
                                else
                                {
                                    if (item.shiftId == 2)
                                    {
                                        int firstshiftrequiredFaculty = (int)Math.Ceiling(facultyCounts.Where(e =>
                                     e.ProposedIntake != 0 && e.ispercentage == false &&
                                     e.specializationId == item.specializationId && e.shiftId == 1)
                                     .Select(e => e.requiredFaculty)
                                     .FirstOrDefault());
                                        item.specializationWiseFaculty = item.specializationWiseFaculty -
                                                                            firstshiftrequiredFaculty;
                                        if (item.specializationWiseFaculty < 0)
                                        {
                                            item.specializationWiseFaculty = 0;
                                        }
                                    }
                                    if (item.specializationWiseFaculty >= rFaculty)
                                    {
                                        if (rFaculty <= item.specializationWiseFaculty)
                                        {
                                            remainingFaculty = remainingFaculty - rFaculty;
                                            adjustedFaculty = rFaculty;
                                        }

                                        else if (rFaculty >= item.specializationWiseFaculty)
                                        {
                                            remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                            adjustedFaculty = item.specializationWiseFaculty;
                                        }
                                    }
                                    else
                                    {
                                        minimumRequirementMet = "NO";
                                        item.deficiency = true;
                                        remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                        adjustedFaculty = item.specializationWiseFaculty;
                                    }
                                }
                            }

                            //if (rFaculty <= item.specializationWiseFaculty)
                            //{
                            //    remainingFaculty = remainingFaculty - rFaculty;
                            //    adjustedFaculty = rFaculty;
                            //    item.BtechAdjustedFaculty = adjustedFaculty;
                            //}

                            //else if (rFaculty >= item.specializationWiseFaculty)
                            //{
                            //    remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                            //    adjustedFaculty = item.specializationWiseFaculty;
                            //    item.deficiency = true;
                            //    item.BtechAdjustedFaculty = adjustedFaculty;
                            //}
                        }
                        else
                        {
                            minimumRequirementMet = "NO";
                            item.deficiency = true;
                            //adjustedFaculty = remainingFaculty;
                            //item.BtechAdjustedFaculty = adjustedFaculty;
                            //facultyShortage = rFaculty - remainingFaculty;
                            //remainingFaculty = 0;
                            if (remainingFaculty >= item.specializationWiseFaculty)
                            {

                                remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                adjustedFaculty = item.specializationWiseFaculty;
                                item.BtechAdjustedFaculty = adjustedFaculty;
                            }
                            else
                            {

                                adjustedFaculty = remainingFaculty;
                                item.BtechAdjustedFaculty = adjustedFaculty;
                                remainingFaculty = 0;
                            }
                        }
                        //remainingPHDFaculty = item.phdFaculty;
                        //if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
                        //{
                        //    //adjustedPHDFaculty = 1;
                        //    item.PHDdeficiency = false;
                        //    adjustedPHDFaculty = remainingPHDFaculty;
                        //    item.AvailablePHDFaculty = 0;
                        //    intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = false);
                        //    remainingPHDFaculty = remainingPHDFaculty - 1;
                        //}
                        //else if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
                        //{
                        //    //adjustedPHDFaculty = 1;
                        //    item.PHDdeficiency = true;
                        //    adjustedPHDFaculty = remainingPHDFaculty;
                        //    item.AvailablePHDFaculty = 2;
                        //    intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = true);
                        //    remainingPHDFaculty = remainingPHDFaculty - 1;
                        //}

                        //New  Code
                        if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")))
                        {
                            //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                            //adjustedPHDFaculty = remainingPHDFaculty;
                            //PhdminimumRequirementMet = "NO";
                            if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                            {
                                if (item.shiftId == 1)
                                {
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }

                                }
                                else
                                {

                                    if (item.shiftId == 2)
                                    {
                                        int firstshiftintake = facultyCounts.Where(e =>
                                         e.ProposedIntake != 0 && e.ispercentage == false &&
                                         e.specializationId == item.specializationId && e.shiftId == 1)
                                         .Select(e => e.approvedIntake1)
                                         .FirstOrDefault();
                                        int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                        item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                         firstshiftPHDFaculty;
                                        if (item.SpecializationsphdFaculty < 0)
                                        {
                                            item.SpecializationsphdFaculty = 0;
                                        }
                                    }
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }

                                }
                            }
                            else
                            {
                                if (item.shiftId == 2)
                                {
                                    int firstshiftintake = facultyCounts.Where(e =>
                                        e.ProposedIntake != 0 && e.ispercentage == false &&
                                        e.specializationId == item.specializationId && e.shiftId == 1)
                                        .Select(e => e.ProposedIntake)
                                        .FirstOrDefault();
                                    int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                    item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                     firstshiftPHDFaculty;
                                    if (item.SpecializationsphdFaculty < 0)
                                    {
                                        item.SpecializationsphdFaculty = 0;
                                    }
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        if (remainingPHDFaculty < 0)
                                        {
                                            remainingPHDFaculty = 0;
                                            adjustedPHDFaculty = remainingPHDFaculty;
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                        else
                                        {
                                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }

                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty -
                                                              item.SpecializationsphdFaculty;
                                        if (remainingPHDFaculty < 0)
                                        {
                                            adjustedPHDFaculty = 0;
                                        }
                                        else
                                        {
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        }

                                        PhdminimumRequirementMet = "NO";
                                    }
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;

                                    if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else
                                    {
                                        PhdminimumRequirementMet = "NO";
                                        remainingPHDFaculty = 0;
                                    }
                                }
                            }

                        }
                        else if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
                        {

                            //if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                            //{
                            //    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                            //    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                            //    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                            //    {
                            //        PhdminimumRequirementMet = "YES";
                            //        item.PHDdeficiency = false;
                            //    }
                            //    else
                            //    {
                            //        PhdminimumRequirementMet = "NO";
                            //        item.PHDdeficiency = true;
                            //    }

                            //}
                            //else
                            //{

                            //    remainingPHDFaculty = remainingPHDFaculty;
                            //    adjustedPHDFaculty = remainingPHDFaculty;

                            //    if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                            //    {
                            //        PhdminimumRequirementMet = "YES";
                            //        item.PHDdeficiency = false;
                            //    }

                            //    else
                            //    {
                            //        PhdminimumRequirementMet = "NO";
                            //        item.PHDdeficiency = true;
                            //    }

                            //}

                            if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                            {
                                if (item.shiftId == 1)
                                {
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            if (adjustedFaculty < adjustedPHDFaculty)
                                            {
                                                adjustedPHDFaculty = adjustedFaculty;
                                            }
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }

                                }
                                else
                                {
                                    if (item.shiftId == 2)
                                    {
                                        int firstshiftintake = facultyCounts.Where(e =>
                                         e.ProposedIntake != 0 && e.ispercentage == false &&
                                         e.specializationId == item.specializationId && e.shiftId == 1)
                                         .Select(e => e.ProposedIntake)
                                         .FirstOrDefault();
                                        int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                        item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                         firstshiftPHDFaculty;
                                        if (item.SpecializationsphdFaculty < 0)
                                        {
                                            item.SpecializationsphdFaculty = 0;
                                        }
                                    }
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }

                                }
                            }
                            else
                            {
                                if (item.shiftId == 2)
                                {
                                    int firstshiftintake = facultyCounts.Where(e =>
                                        e.ProposedIntake != 0 && e.ispercentage == false &&
                                        e.specializationId == item.specializationId && e.shiftId == 1)
                                        .Select(e => e.ProposedIntake)
                                        .FirstOrDefault();
                                    int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                    item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                     firstshiftPHDFaculty;
                                    if (item.SpecializationsphdFaculty < 0)
                                    {
                                        item.SpecializationsphdFaculty = 0;
                                    }
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty -
                                                              item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }
                                }
                                else
                                {
                                    if (remainingPHDFaculty == SpecializationwisePHDFaculty)
                                    {
                                        adjustedPHDFaculty = remainingPHDFaculty;
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty;
                                        adjustedPHDFaculty = remainingPHDFaculty;
                                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                }
                            }
                        }
                        else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
                        {

                            adjustedPHDFaculty = remainingPHDFaculty;

                            remainingPHDFaculty = remainingPHDFaculty - 1;
                            PhdminimumRequirementMet = "NO";
                            item.PHDdeficiency = true;
                        }
                        else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty > 0)
                        {

                            if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                            {
                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                PhdminimumRequirementMet = "YES";
                                item.PHDdeficiency = false;
                            }
                            else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                            {
                                remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                PhdminimumRequirementMet = "YES";
                                item.PHDdeficiency = false;
                            }

                        }
                        else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                        {

                            adjustedPHDFaculty = remainingPHDFaculty;
                            PhdminimumRequirementMet = "NO";
                            item.PHDdeficiency = true;
                        }
                        else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                        {
                            PhdminimumRequirementMet = "NO";
                            item.PHDdeficiency = true;
                        }
                        else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                        {
                            PhdminimumRequirementMet = "YES";
                            item.PHDdeficiency = false;
                        }
                        //Dual Degree
                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
                        {

                            if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                            {
                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                {
                                    PhdminimumRequirementMet = "YES";
                                    item.PHDdeficiency = false;
                                }

                                else
                                {
                                    item.PHDdeficiency = true;
                                    PhdminimumRequirementMet = "NO";
                                }
                            }
                            else
                            {
                                remainingPHDFaculty = remainingPHDFaculty;
                                adjustedPHDFaculty = remainingPHDFaculty;

                                if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                {
                                    item.PHDdeficiency = false;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else
                                {
                                    item.PHDdeficiency = true;
                                    PhdminimumRequirementMet = "NO";
                                }

                            }

                        }
                        else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
                        {

                            adjustedPHDFaculty = remainingPHDFaculty;

                            remainingPHDFaculty = remainingPHDFaculty - 1;
                            PhdminimumRequirementMet = "NO";
                            item.PHDdeficiency = true;
                        }
                        else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("Dual Degree")))
                        {
                            PhdminimumRequirementMet = "YES";
                            item.PHDdeficiency = false;
                        }
                        else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("Dual Degree")))
                        {
                            PhdminimumRequirementMet = "NO";
                            item.PHDdeficiency = true;
                        }


                    }
                    if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                    {
                        if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "-";
                            item.deficiency = false;
                            item.PHDdeficiency = false;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "Deficiency In faculty";
                            item.deficiency = true;

                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "Deficiency In Ph.D";
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else
                        {
                            //  minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            item.deficiency = true;
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                    }
                    else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                    {

                        if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "-";
                            item.deficiency = false;
                            item.PHDdeficiency = false;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            //  minimumRequirementMet = "Deficiency In faculty";
                            item.deficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "Deficiency In Ph.D";
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else
                        {
                            //  minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            item.deficiency = true;
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }

                    }
                    else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                    {

                        if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "-";
                            item.deficiency = false;
                            item.PHDdeficiency = false;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            //  minimumRequirementMet = "Deficiency In faculty";
                            item.deficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "Deficiency In Ph.D";
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else
                        {
                            item.deficiency = true;
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                            // minimumRequirementMet = "Deficiency In faculty and Ph.D";
                        }

                    }
                    else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                    {

                        if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "-";
                            item.deficiency = false;
                            item.PHDdeficiency = false;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            //  minimumRequirementMet = "Deficiency In faculty";
                            item.deficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "Deficiency In Ph.D";
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else
                        {
                            item.deficiency = true;
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                            // minimumRequirementMet = "Deficiency In faculty and Ph.D";
                        }

                    }
                    else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                    {

                        if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            //  minimumRequirementMet = "-";
                            item.deficiency = false;
                            item.PHDdeficiency = false;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        {
                            //  minimumRequirementMet = "Deficiency In faculty";
                            item.deficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                        {
                            // minimumRequirementMet = "Deficiency In Ph.D";
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                        else
                        {
                            //  minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            item.deficiency = true;
                            item.PHDdeficiency = true;
                            item.Testrequriedfaculty = rFaculty;
                            item.Testavilablefaculty = adjustedFaculty;
                        }
                    }


                    if (strOtherDepartments.Contains(item.Department))
                    {
                        if (OthersSpecIds.Contains(item.DepartmentID))
                        {
                            item.totalIntake = totalBtechFirstYearIntake;
                            //item.requiredFaculty = Math.Ceiling((decimal)othersRequiredfaculty);
                            double rid = (double)(firstYearRequired / 2);
                            rFaculty = (int)(Math.Ceiling(rid));
                            item.requiredFaculty = Math.Ceiling((decimal)rFaculty);
                        }
                        else
                        {
                            item.totalIntake = totalBtechFirstYearIntake;
                            item.requiredFaculty = Math.Ceiling((decimal)firstYearRequired);
                        }
                    }
                    else
                    {
                        if (item.Degree == "B.Tech")
                        {
                            item.division1 = GetSectionBasedonIntake(item.approvedIntake2);
                            item.division2 = GetSectionBasedonIntake(item.approvedIntake3);
                            item.division3 = GetSectionBasedonIntake(item.approvedIntake4);
                        }
                    }
                    deptloop++;
                }
            #endregion
                #region Faculty Appeal Deficiency Status
                var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.Where(r => r.academicYearId == 11).ToList();
                var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
                foreach (var item in intakedetailsList.Where(i => i.shiftId == 1).ToList())
                {

                    var deparment = jntuh_deparment.FirstOrDefault(i => i.id == item.DepartmentID);
                    if (deparment != null)
                    {
                        var jntuh_departmentcount = jntuh_appeal_faculty.Where(i => i.DepartmentId == item.DepartmentID &&
                                i.DegreeId == deparment.degreeId && i.collegeId == collegeId && i.NOtificationReport != null).ToList();//&& i.SpecializationId == item.specializationId
                        var facultydefcount = (int)Math.Ceiling(item.requiredFaculty) - item.BtechAdjustedFaculty;
                        if (item.PHDdeficiency == true)
                        {
                            facultydefcount = facultydefcount + item.AvailablePHDFaculty;
                        }
                        //if (facultydefcount <= jntuh_departmentcount.Count && jntuh_departmentcount.Count != 0)
                        //{
                        //    item.deficiencystatus = true;
                        //}

                        if (jntuh_departmentcount.Count >= 2 && jntuh_departmentcount.Count != 0)
                        {
                            item.deficiencystatus = true;
                        }


                    }
                }


                #endregion


                #region Principal Appeal Deficiency Status

                var jntuhAppealPrincipal = db.jntuh_appeal_principal_registered.Where(p => p.academicYearId == 11).ToList();
                var prinipal = jntuhAppealPrincipal.Where(i => i.collegeId == collegeId).ToList();
                if (prinipal.Count > 0)
                {
                    ViewBag.principaldeficiencystatus = true;
                }
                else
                {
                    ViewBag.principaldeficiencystatus = false;
                }
                #endregion

            }
            return View(intakedetailsList.ToList());

        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult AppealReverificationFaculty(string type, string CollegeId)
        {
            #region new code
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int Collegeid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"])); ;
            if (Collegeid == 375)
            {
                Collegeid = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == Collegeid).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            var FacultyRegistrationList = new List<FacultyRegistration>();
            if (Collegeid != null)
            {

                //var department = db.jntuh_department.AsNoTracking().FirstOrDefault(i => i.id == departmentid).departmentName.ToUpper();
                //if (department.Contains("CIVIL"))
                //{
                //    departmentid = 5;
                //}
                //else if (department.Contains("ECE"))
                //{
                //    departmentid = 2;
                //}
                //else if (department.Contains("EEE"))
                //{
                //    departmentid = 1;
                //}
                //else if (department.Contains("MECHANICAL"))
                //{
                //    departmentid = 15;
                //}
                //else if (department.Contains("CSE"))
                //{
                //    departmentid = 3;
                //}

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == Collegeid).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();


                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == Collegeid).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList()
                   : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& rf.RegistrationNumber != principalRegno && rf.DepartmentId == departmentid


                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                //var jntuh_registered_faculty1 = registeredFaculty.Select(rf => new
                //{
                //    type = rf.type,
                //    Absent = rf.Absent != null ? (bool)rf.Absent : false,
                //    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE != null ? (bool)rf.NotQualifiedAsperAICTE : false,
                //    InvalidPANNo = rf.InvalidPANNumber != null ? (bool)rf.InvalidPANNumber : false,
                //    InCompleteCeritificates = rf.IncompleteCertificates != null ? (bool)rf.IncompleteCertificates : false,
                //    PANNumber = rf.PANNumber,
                //    XeroxcopyofcertificatesFlag = rf.Xeroxcopyofcertificates != null ? (bool)rf.Xeroxcopyofcertificates : false,
                //    NOrelevantUgFlag = rf.NoRelevantUG == "Yes" ? true : false,
                //    NOrelevantPgFlag = rf.NoRelevantPG == "Yes" ? true : false,
                //    NOrelevantPhdFlag = rf.NORelevantPHD == "Yes" ? true : false,
                //    BlacklistFaculty = rf.Blacklistfaculy != null ? (bool)rf.Blacklistfaculy : false,
                //    NotIdentityFiedForAnyProgramFlag = rf.NotIdentityfiedForanyProgram != null ? (bool)rf.NotIdentityfiedForanyProgram : false,
                //    OriginalCertificatesnotshownFlag = rf.OriginalCertificatesNotShown != null ? (bool)rf.OriginalCertificatesNotShown : false,
                //    NoSCM = rf.NoSCM != null ? (bool)rf.NoSCM : false,
                //    SamePANUsedByMultipleFaculty = rf.Invaliddegree != null ? (bool)(rf.Invaliddegree) : false,
                //    BASStatusOld = rf.BAS,
                //    BASStatus = rf.InvalidAadhaar,
                //    OriginalsVerifiedUG = rf.OriginalsVerifiedUG == true ? true : false,
                //    OriginalsVerifiedPHD = rf.OriginalsVerifiedPHD == true ? true : false,

                //    // NoSCM = rf.NoSCM17 != null ? (bool)rf.NoSCM17 : false,
                //    // FalsePAN = rf.FalsePAN != null ? (bool)rf.FalsePAN : false,
                //    // NOForm16 = rf.NoForm16 != null ? (bool)rf.NoForm16 : false,
                //    // MultipleReginSamecoll = rf.MultipleRegInSameCollege != null ? (bool)rf.MultipleRegInSameCollege : false,

                //    //NoForm16Verification = rf.Noform16Verification != null ? (bool)rf.Noform16Verification : false,
                //    //PhotocopyofPAN = rf.PhotoCopyofPAN != null ? (bool)rf.PhotoCopyofPAN : false,
                //    PhdUndertakingDocumentstatus = rf.PhdUndertakingDocumentstatus != null ? (bool)(rf.PhdUndertakingDocumentstatus) : false,
                //    PHDUndertakingDocumentView = rf.PHDUndertakingDocument,
                //    PhdUndertakingDocumentText = rf.PhdUndertakingDocumentText,
                //    // AppliedPAN = rf.AppliedPAN != null ? (bool)(rf.AppliedPAN) : false,
                //    Notin116 = rf.Notin116,
                //    Blacklistfaculy = rf.Blacklistfaculy,
                //    RegistrationNumber = rf.RegistrationNumber,
                //    Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : "",
                //    HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                //    IsApproved = rf.isApproved,
                //    PanNumber = rf.PANNumber,
                //    AadhaarNumber = rf.AadhaarNumber,
                //    Photo = rf.Photo,
                //    FullName = rf.FirstName + rf.MiddleName + rf.LastName,
                //    FacultyEducation = rf.jntuh_registered_faculty_education,
                //    DegreeId = rf.jntuh_registered_faculty_education.Count(e => e.facultyId == rf.id) > 0 ? rf.jntuh_registered_faculty_education.Where(e => e.facultyId == rf.id).Select(e => e.educationId).Max() : 0,
                //    DepartmentId = rf.DepartmentId



                //}).ToList();

                var RegistrationNumbersCleared = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) &&
                                                 rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) &&  // (rf.AppliedPAN == false || rf.AppliedPAN == null) && && (rf.MultipleReginSamecoll == false || rf.MultipleReginSamecoll == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) 
                                                  (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && rf.BAS != "Yes" && rf.InvalidAadhaar != "Yes" && (rf.OriginalsVerifiedUG == false || rf.OriginalsVerifiedUG == null) && (rf.OriginalsVerifiedPHD == false || rf.OriginalsVerifiedPHD == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))).Select(e => e.RegistrationNumber).ToArray();


                var jntuh_registered_faculty = registeredFaculty.Where(e => !RegistrationNumbersCleared.Contains(e.RegistrationNumber)).Select(rf => rf).ToList();


                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration facultyregistered = new FacultyRegistration();
                    facultyregistered.id = item.id;
                    facultyregistered.RegistrationNumber = item.RegistrationNumber.Trim();
                    var collegefaculty =
                        db.jntuh_college_faculty_registered.Where(
                            r => r.RegistrationNumber == facultyregistered.RegistrationNumber.Trim())
                            .Select(s => s)
                            .FirstOrDefault();
                    facultyregistered.FirstName = item.FirstName;
                    facultyregistered.MiddleName = item.MiddleName;
                    facultyregistered.LastName = item.LastName;
                    facultyregistered.DepartmentId = collegefaculty.DepartmentId;
                    facultyregistered.department = facultyregistered.DepartmentId != null ? db.jntuh_department.Select(s => s.departmentName).FirstOrDefault() : string.Empty;
                    if (collegefaculty.SpecializationId != null)
                    {
                        facultyregistered.SpecializationId = collegefaculty.SpecializationId;
                        facultyregistered.SpecializationName = db.jntuh_specialization.Where(r => r.id == facultyregistered.SpecializationId).Select(s => s.specializationName).FirstOrDefault();
                    }
                    //facultyregistered.DepartmentId = item.DeptId;
                    facultyregistered.jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(e => e.facultyId == facultyregistered.id && e.educationId != 8).Select(s => s).ToList();

                    facultyregistered.facultyPhoto = item.Photo;
                    //facultyregistered.Absent = item.Absent != null && (bool)item.Absent;
                    //facultyregistered.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null && (bool)item.NotQualifiedAsperAICTE;
                    //facultyregistered.NoSCM = item.NoSCM != null && (bool)item.NoSCM;
                    //facultyregistered.PANNumber = item.PANNumber;
                    //facultyregistered.PHDundertakingnotsubmitted = item.PHDundertakingnotsubmitted != null && (bool)item.PHDundertakingnotsubmitted;
                    //facultyregistered.BlacklistFaculty = item.Blacklistfaculy != null && (bool)item.Blacklistfaculy;

                    facultyregistered.DegreeId = facultyregistered.jntuh_registered_faculty_education.Count(e => e.facultyId == item.id) > 0 ? facultyregistered.jntuh_registered_faculty_education.Where(e => e.facultyId == item.id).Select(e => e.educationId).Max() : 0;

                    if (item.Absent == true)
                        Reason += "Absent";
                    if (item.type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }
                    if (item.Xeroxcopyofcertificates == true)
                    {
                        if (Reason != null)
                            Reason += ",Photo copy of Certificates";
                        else
                            Reason += "Photo copy of Certificates";
                    }
                    if (item.NoRelevantUG == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant UG";
                        else
                            Reason += "NO Relevant UG";
                    }

                    if (item.NoRelevantPG == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PG";
                        else
                            Reason += "NO Relevant PG";
                    }

                    if (item.NORelevantPHD == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PHD";
                        else
                            Reason += "NO Relevant PHD";
                    }

                    if (item.NotQualifiedAsperAICTE == true || facultyregistered.DegreeId < 4)
                    {
                        if (Reason != null)
                            Reason += ",Not Qualified as per AICTE/PCI";
                        else
                            Reason += "Not Qualified as per AICTE/PCI";
                    }

                    if (item.InvalidPANNumber == true)
                    {
                        if (Reason != null)
                            Reason += ",InvalidPAN";
                        else
                            Reason += "InvalidPAN";
                    }
                    if (item.NotconsideredPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                        else
                            Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                    }
                    if (item.IncompleteCertificates == true)
                    {
                        if (Reason != null)
                            Reason += ",IncompleteCertificates";
                        else
                            Reason += "IncompleteCertificates";
                    }
                    if (item.Genuinenessnotsubmitted == true)
                    {
                        if (Reason != null)
                            Reason += ",PHD Genuinity not Submitted";
                        else
                            Reason += "PHD Genuinity not Submitted";
                    }
                    if (item.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",no/not valid SCM";
                        else
                            Reason += "no/not valid SCM";
                    }
                    if (item.OriginalCertificatesNotShown == true)
                    {
                        if (Reason != null)
                            Reason += ",Orginal Certificates Not shown in College Inspection";
                        else
                            Reason += "Orginal Certificates Not shown in College Inspection";
                    }
                    if (item.PANNumber == null)
                    {
                        if (Reason != null)
                            Reason += ",No PAN Number";
                        else
                            Reason += "No PAN Number";
                    }
                    if (item.Invaliddegree == true)
                    {
                        if (Reason != null)
                            Reason += ",AICTE Not Approved University Degrees";
                        else
                            Reason += "AICTE Not Approved University Degrees";
                    }
                    if (item.BAS == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",BAS Flag";
                        else
                            Reason += "BAS Flag";
                    }
                    if (item.InvalidAadhaar == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",Invalid/Blur Aadhaar";
                        else
                            Reason += "Invalid/Blur Aadhaar";
                    }

                    if (item.OriginalsVerifiedUG == true)
                    {
                        if (Reason != null)
                            Reason += ",Complaint PHD Faculty";
                        else
                            Reason += "Complaint PHD Faculty";
                    }

                    if (item.OriginalsVerifiedPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",No Guide Sign in PHD Thesis";
                        else
                            Reason += "No Guide Sign in PHD Thesis";
                    }
                    if (item.Blacklistfaculy == true)
                    {
                        if (Reason != null)
                            Reason += ",Blacklisted Faculty";
                        else
                            Reason += "Blacklisted Faculty";
                    }
                    if (item.NoPGspecialization == true)
                    {
                        if (Reason != null)
                            Reason += ",no Specialization in PG";
                        else
                            Reason += "no Specialization in PG";
                    }
                    if (item.Noclass == true)
                    {
                        if (Reason != null)
                            Reason += ",no Class in UG/PG";
                        else
                            Reason += "no Class in UG/PG";
                    }
                    facultyregistered.DeactivationReason = Reason;
                    FacultyRegistrationList.Add(facultyregistered);
                }
            }

            ViewBag.collegeid = Collegeid;
            //ViewBag.departmentid = departmentid;
            //ViewBag.degree = degree;
            //ViewBag.specializationid = specializationid;
            //ViewBag.deficiencycount = deficencycount;
            return View(FacultyRegistrationList);
            #endregion

        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult AppealCollegeFacultyDetails()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        // return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    //return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            List<jntuh_appeal_faculty_registered> facultydetails = new List<jntuh_appeal_faculty_registered>();
            if (collegeId != 0)
            {
                facultydetails = db.jntuh_appeal_faculty_registered.Where(i => i.collegeId == collegeId && i.academicYearId == prAy).Select(i => i).ToList();
            }
            return View(facultydetails);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AddAppealReverificationFaculty(string fid)
        {
            //string collegeId, string fid, int deficencycount, int departmentid, string degree, string specializationid, string registrationnumber
            int facultyId = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    //return RedirectToAction("CollegeDashboard", "Dashboard");
                    return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }

            if (!string.IsNullOrEmpty(fid))
            {
                facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            else
            {
                return RedirectToAction("AppealReverificationFaculty", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            var facultydetails =
                db.jntuh_registered_faculty.Where(r => r.id == facultyId).Select(s => s).FirstOrDefault();
            var collegefaculty =
                db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == facultydetails.RegistrationNumber.Trim()).Select(s => s).FirstOrDefault();
            ViewBag.registrationnumber = facultydetails.RegistrationNumber.Trim();
            CollegeFaculty faculty = new CollegeFaculty();
            faculty.FacultyRegistrationNumber = facultydetails.RegistrationNumber.Trim();
            faculty.collegeId = Convert.ToInt16(collegefaculty.collegeId);
            // faculty.DegreeName = degree;
            if (collegefaculty.SpecializationId != null)
            {
                faculty.SpecializationId = Convert.ToInt16(collegefaculty.SpecializationId);
            }

            faculty.facultyDepartmentId = Convert.ToInt32(collegefaculty.DepartmentId);
            //faculty.Facultydeficencycount = collegefaculty.DepartmentId;
            // var firstOrDefault = db.jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == registrationnumber).Select(e => e.SpecializationId).FirstOrDefault();
            if (collegefaculty.SpecializationId != null)
                faculty.FacultyAddedSpecializationId = (int)collegefaculty.SpecializationId;


            List<SpecializationList> Specializations = new List<SpecializationList>();


            Specializations = (from t in db.jntuh_college_intake_existing
                               join cf in db.jntuh_specialization on t.specializationId equals cf.id
                               join dd in db.jntuh_department on cf.departmentId equals dd.id
                               join de in db.jntuh_degree on dd.degreeId equals de.id
                               where t.collegeId == userCollegeID && t.academicYearId == 11 && t.courseStatus != "Closure" && t.proposedIntake != 0
                               select new SpecializationList
                               {
                                   SpecializationId = cf.id,
                                   SpecializationName = de.degree + "-" + cf.specializationName

                               }).ToList();

            Specializations.Add(new SpecializationList() { SpecializationId = 155, SpecializationName = "BTech-Others(CSE/IT)" });
            Specializations.Add(new SpecializationList() { SpecializationId = 156, SpecializationName = "BTech-Others(CIVIL/MECH)" });
            Specializations.Add(new SpecializationList() { SpecializationId = 157, SpecializationName = "BTech-Others(ECE/EEE)" });
            Specializations.Add(new SpecializationList() { SpecializationId = 158, SpecializationName = "BTech-Others(MNGT/H&S)" });
            Specializations.Add(new SpecializationList() { SpecializationId = 48, SpecializationName = "BTech-Physics" });
            Specializations.Add(new SpecializationList() { SpecializationId = 42, SpecializationName = "BTech-Mathematics" });
            Specializations.Add(new SpecializationList() { SpecializationId = 37, SpecializationName = "BTech-English" });
            Specializations.Add(new SpecializationList() { SpecializationId = 31, SpecializationName = "BTech-Chemistry" });
            ViewBag.Specializations = Specializations;

            faculty.AadhaarFlag =
                db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == facultydetails.RegistrationNumber.Trim())
                    .Select(s => s.InvalidAadhaar)
                    .FirstOrDefault();

            //var jntuh_college_faculty_registered =
            //    db.jntuh_college_faculty_registered.Where(s => s.RegistrationNumber == registrationnumber)
            //        .Select(s => s)
            //        .FirstOrDefault();
            //faculty.facultyAadhaarNumber = jntuh_college_faculty_registered.AadhaarNumber;
            //faculty.facultyAadharDocument = jntuh_college_faculty_registered.AadhaarDocument;


            //ViewBag.IdentityFied =db.jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == registrationnumber).Select(e => e.IdentifiedFor).FirstOrDefault()!=null?true:false;
            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddAppealReverificationFaculty(CollegeFaculty faculty)
        {
            TempData["Error"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.Where(r => r.academicYearId == prAy).ToList();
            var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
            string[] RegistrationNumber = jntuh_appeal_faculty.Select(i => i.RegistrationNumber).ToArray();
            if (String.IsNullOrEmpty(faculty.FacultyRegistrationNumber) || isRegisteredFaculty == null)
            {
                TempData["Error"] = "Invalid Faculty Registration Number.";
                return RedirectToAction("AppealReverificationFaculty", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            var physicalpresencpath = "~/Content/Upload/OnlineAppealDocuments/Faculty/PhysicalPresenceReports";
            var appealreverificationSupportdoc = "~/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationSupportReports";
            var AppealFacultyAadhaarDocuments = "~/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments";
            var deparment = jntuh_deparment.FirstOrDefault(i => i.id == faculty.facultyDepartmentId);
            if (deparment != null)
            {
                var jntuh_departmentcount = jntuh_appeal_faculty.Where(i =>
                        i.DepartmentId == faculty.facultyDepartmentId && i.SpecializationId == faculty.SpecializationId && i.DegreeId == deparment.degreeId).ToList();

                if (faculty.Facultydeficencycount > jntuh_departmentcount.Count)
                {

                }
            }
            if (!RegistrationNumber.Contains(faculty.FacultyRegistrationNumber.Trim()))
            {
                //int FacultyId = db.jntuh_college_faculty_registered.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(F => F.id).FirstOrDefault();
                //jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(i => i).FirstOrDefault();

                jntuh_appeal_faculty_registered UpdatedFaculty = new jntuh_appeal_faculty_registered();
                UpdatedFaculty.collegeId = isRegisteredFaculty.collegeId != null
                    ? (int)isRegisteredFaculty.collegeId
                    : 0;
                UpdatedFaculty.collegeId = faculty.collegeId;
                UpdatedFaculty.RegistrationNumber = faculty.FacultyRegistrationNumber.Trim();
                UpdatedFaculty.academicYearId = prAy;
                UpdatedFaculty.existingFacultyId = isRegisteredFaculty.id;
                UpdatedFaculty.IdentifiedFor = faculty.facultyRecruitedFor;
                //Old Code
                //UpdatedFaculty.DepartmentId = faculty.facultyDepartmentId;
                //UpdatedFaculty.SpecializationId = faculty.SpecializationId;
                UpdatedFaculty.SpecializationId = faculty.FacultyAddedSpecializationId;
                var jntuhSpecialization = db.jntuh_specialization.Where(i => i.id == faculty.FacultyAddedSpecializationId).FirstOrDefault();
                if (jntuhSpecialization != null)
                {
                    UpdatedFaculty.DepartmentId = jntuhSpecialization.departmentId;

                    var jntuhDepartment = jntuh_deparment.Where(i => i.id == jntuhSpecialization.departmentId).FirstOrDefault();
                    if (jntuhDepartment != null)
                    {
                        UpdatedFaculty.DegreeId = jntuhDepartment.degreeId;
                    }
                }
                if (UpdatedFaculty.DegreeId == 4 || UpdatedFaculty.DegreeId == 5)
                {
                    UpdatedFaculty.IdentifiedFor = "UG";
                    UpdatedFaculty.SpecializationId = null;
                }
                else
                {
                    UpdatedFaculty.IdentifiedFor = "PG";
                }
                //New Code Aadhaar Faculty
                UpdatedFaculty.AadhaarNumber = faculty.facultyAadhaarNumber == null ? faculty.facultyAadhaarNumber : faculty.facultyAadhaarNumber.Trim();
                const int DelayOnRetry = 3000;
                try
                {
                    if (faculty.facultyAadharPhotoDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(AppealFacultyAadhaarDocuments)))
                        {
                            Directory.CreateDirectory(Server.MapPath(AppealFacultyAadhaarDocuments));
                        }
                        var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);
                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(AppealFacultyAadhaarDocuments),
                                fileName, ext));
                            UpdatedFaculty.AadhaarDocument = string.Format("{0}/{1}{2}", AppealFacultyAadhaarDocuments,
                                fileName, ext);
                        }
                    }
                    if (!String.IsNullOrEmpty(faculty.facultyAadharDocument))
                    {
                        UpdatedFaculty.AadhaarDocument = faculty.facultyAadharDocument;
                    }
                    if (faculty.PhysicalPresenceDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(physicalpresencpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(physicalpresencpath));
                        }

                        var ext = Path.GetExtension(faculty.PhysicalPresenceDocument.FileName);
                        if (ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.PhysicalPresenceDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(physicalpresencpath), fileName, ext));
                            UpdatedFaculty.PhysicalPresenceonInspection = string.Format("{0}/{1}{2}",
                                physicalpresencpath, fileName, ext);
                        }
                    }

                    //544444444444444
                    if (faculty.AppealReverificationSupportDoc != null)
                    {
                        if (!Directory.Exists(Server.MapPath(appealreverificationSupportdoc)))
                        {
                            Directory.CreateDirectory(Server.MapPath(appealreverificationSupportdoc));
                        }

                        var ext = Path.GetExtension(faculty.AppealReverificationSupportDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.AppealReverificationSupportDoc.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(appealreverificationSupportdoc), fileName, ext));
                            UpdatedFaculty.AppealReverificationSupportingDocument = string.Format("{0}/{1}{2}",
                                appealreverificationSupportdoc, fileName, ext);
                        }
                    }
                }
                catch (IOException e)
                {
                    Thread.Sleep(DelayOnRetry);
                }
                UpdatedFaculty.createdOn = DateTime.Now;
                UpdatedFaculty.createdBy = userID;
                db.jntuh_appeal_faculty_registered.Add(UpdatedFaculty);
                db.SaveChanges();
                //This code is written for certificates missing at the time of Reverification Faculty adding written by Naryana Reddy
                var checkregdata =
                    db.jntuh_appeal_faculty_registered.Where(
                        r =>
                            r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim() && r.academicYearId == prAy &&
                            r.AppealReverificationSupportingDocument!=null).Select(s => s).FirstOrDefault();
                if (checkregdata == null)
                {
                    var removereg =
                    db.jntuh_appeal_faculty_registered.Where(
                        r =>
                            r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim() && r.academicYearId == prAy).Select(s => s).FirstOrDefault();
                    if (removereg != null)
                    {
                        db.jntuh_appeal_faculty_registered.Remove(removereg);
                        db.SaveChanges();
                        TempData["Success"] = null;
                        TempData["Error"] = "Faculty Registration Number Failed for Re-verification.";
                    }
                }
                else
                {
                    TempData["Success"] = "Faculty Registration Number Successfully Added for Re-verification.";
                    TempData["Error"] = null;
                }
            }
            else
            {
                var facultydata = db.jntuh_appeal_faculty_registered.Where(i => i.RegistrationNumber == faculty.FacultyRegistrationNumber && i.academicYearId == prAy).FirstOrDefault();
                const int DelayOnRetry = 3000;
                try
                {
                    if (faculty.facultyAadharPhotoDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(AppealFacultyAadhaarDocuments)))
                        {
                            Directory.CreateDirectory(Server.MapPath(AppealFacultyAadhaarDocuments));
                        }
                        var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);
                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(AppealFacultyAadhaarDocuments),
                                fileName, ext));
                            facultydata.AadhaarDocument = string.Format("{0}/{1}{2}", AppealFacultyAadhaarDocuments,
                                fileName, ext);
                        }
                    }

                    if (faculty.PhysicalPresenceDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(physicalpresencpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(physicalpresencpath));
                        }

                        var ext = Path.GetExtension(faculty.PhysicalPresenceDocument.FileName);
                        if (ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" + faculty.FacultyRegistrationNumber + "_" +
                                DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.PhysicalPresenceDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(physicalpresencpath), fileName, ext));
                            facultydata.PhysicalPresenceonInspection = string.Format("{0}/{1}{2}", physicalpresencpath,
                                fileName, ext);
                        }


                    }
                    if (faculty.AppealReverificationSupportDoc != null)
                    {
                        if (!Directory.Exists(Server.MapPath(appealreverificationSupportdoc)))
                        {
                            Directory.CreateDirectory(Server.MapPath(appealreverificationSupportdoc));
                        }

                        var ext = Path.GetExtension(faculty.AppealReverificationSupportDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.AppealReverificationSupportDoc.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(appealreverificationSupportdoc),
                                fileName, ext));
                            facultydata.AppealReverificationSupportingDocument = string.Format("{0}/{1}{2}",
                                appealreverificationSupportdoc, fileName, ext);
                        }
                    }
                }
                catch (IOException e)
                {
                    Thread.Sleep(DelayOnRetry);
                }

                facultydata.SpecializationId = faculty.FacultyAddedSpecializationId;
                var jntuhSpecialization = db.jntuh_specialization.Where(i => i.id == faculty.FacultyAddedSpecializationId).FirstOrDefault();
                if (jntuhSpecialization != null)
                {
                    facultydata.DepartmentId = jntuhSpecialization.departmentId;



                    var jntuhDepartment = jntuh_deparment.Where(i => i.id == jntuhSpecialization.departmentId).FirstOrDefault();
                    if (jntuhDepartment != null)
                    {
                        facultydata.DegreeId = jntuhDepartment.degreeId;
                    }
                }
                if (facultydata.DegreeId == 4 || facultydata.DegreeId == 5)
                {
                    facultydata.IdentifiedFor = "UG";
                    facultydata.SpecializationId = null;
                }
                else
                {
                    facultydata.IdentifiedFor = "PG";
                }

                facultydata.updatedBy = userID;
                facultydata.updatedOn = DateTime.Now;
                db.Entry(facultydata).State = EntityState.Modified;
                db.SaveChanges();
                //This code is written for certificates missing at the time of Reverification Faculty adding written by Naryana Reddy
                var checkregdata =
                    db.jntuh_appeal_faculty_registered.Where(
                        r =>
                            r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim() && r.academicYearId == prAy &&
                            r.AppealReverificationSupportingDocument != null).Select(s => s).FirstOrDefault();
                if (checkregdata == null)
                {
                    var removereg =
                    db.jntuh_appeal_faculty_registered.Where(
                        r =>
                            r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim() && r.academicYearId == prAy).Select(s => s).FirstOrDefault();
                    if (removereg != null)
                    {
                        db.jntuh_appeal_faculty_registered.Remove(removereg);
                        db.SaveChanges();
                        TempData["Success"] = null;
                        TempData["Error"] = "Faculty Registration Number Failed for Re-verification.";
                    }
                }
                else
                {
                    TempData["Success"] = "Faculty Registration Number Successfully updated for Re-verification..";
                    TempData["Error"] = null;
                }

            }
            return RedirectToAction("AppealReverificationFaculty", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        //Adding Appeal Complance Faculty
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AddComplianceFaculty(string collegeId, string fid, int deficencycount, int departmentid, string degree, int specializationid)
        {
            int facultyId = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            var specializatioName = "";
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.degree = degree;
            ViewBag.deficiencycount = deficencycount;
            ViewBag.specializationid = specializationid;
            ViewBag.departmentid = departmentid;
            //var jntuhSpecialization = db.jntuh_specialization.FirstOrDefault(i => i.id == specializationid);
            //if (jntuhSpecialization != null)
            //{
            //    specializatioName =  jntuhSpecialization.specializationName;
            //}
            var jntuh_degree = db.jntuh_degree.Where(e => e.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();

            CollegeFaculty faculty = new CollegeFaculty();
            if (degree == "B.Tech" || degree == "B.Pharmacy")
            {
                faculty.facultyDepartmentId = departmentid;
                faculty.department = jntuh_department.Where(e => e.id == departmentid).Select(e => e.jntuh_degree.degree + " -" + e.departmentName).FirstOrDefault();
            }
            else
            {
                faculty.facultyPGDepartmentId = departmentid;
                faculty.department = jntuh_department.Where(e => e.id == departmentid).Select(e => e.jntuh_degree.degree + " -" + e.departmentName).FirstOrDefault();
            }
            faculty.SpecializationId = specializationid;
            faculty.SpecializationName = specializationid == null ? null : jntuh_specialization.Where(e => e.id == specializationid).Select(w => w.specializationName).FirstOrDefault();
            faculty.DegreeName = degree;
            if (degree == "B.Tech" || degree == "B.Pharmacy")
            {
                faculty.facultyRecruitedFor = "UG";
            }
            else
            {
                faculty.facultyRecruitedFor = "PG";
            }
            faculty.Facultydeficencycount = deficencycount;
            faculty.collegeId = Convert.ToInt16(collegeId);

            //SpecializationName = specializatioName

            var collegeID = Convert.ToInt16(collegeId);



            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();

            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3) && e.courseStatus != "Closure").Select(e => e).ToList();

            var SpecalizationIds = jntuh_college_intake_existing.Select(e => e.specializationId).Distinct().ToList();

            var DepartmentsData = (from s in jntuh_specialization
                                   join d in jntuh_department on s.departmentId equals d.id
                                   join de in jntuh_degree on d.degreeId equals de.id
                                   where SpecalizationIds.Contains(s.id)
                                   select new Departments
                                   {
                                       DegreeTypeId = de.degreeTypeId,
                                       DegreeId = de.id,
                                       Degree = de.degree,
                                       Department = de.degree + "-" + d.departmentName,
                                       DepartmentId = d.id,
                                       Specialization = s.specializationName,
                                       SpecializationId = s.id

                                   }).ToList();

            DepartmentsData.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 65, Department = "Others(CSE/IT)" });
            DepartmentsData.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 66, Department = "Others(CIVIL/MECH)" });
            DepartmentsData.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 67, Department = "Others(ECE/EEE)" });
            DepartmentsData.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 68, Department = "Others(MNGT/H&S)" });
            DepartmentsData.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 29, Department = "Physics" });
            DepartmentsData.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 30, Department = "Mathematics" });
            DepartmentsData.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 31, Department = "English" });
            DepartmentsData.Add(new Departments() { DegreeTypeId = 1, DepartmentId = 32, Department = "Chemistry" });


            ViewBag.UGDept = DepartmentsData.Where(e => e.DegreeTypeId == 1).Select(
                e => new
                {
                    UgDeptid = e.DepartmentId,
                    UgDeptName = e.Department
                }).Distinct().ToList();

            ViewBag.PGDept = DepartmentsData.Where(e => e.DegreeTypeId == 2).Select(
               e => new
               {
                   PgDeptid = e.DepartmentId,
                   PgDeptName = e.Department
               }).Distinct().ToList();

            ViewBag.DualDept = DepartmentsData.Where(e => e.DegreeTypeId == 3).Select(
              e => new
              {
                  DualDeptid = e.DepartmentId,
                  DualDeptName = e.Department
              }).Distinct().ToList();

            var MPharmacyFacultySpecialization = (from s in jntuh_specialization
                                                  join d in jntuh_department on s.departmentId equals d.id
                                                  join de in jntuh_degree on d.degreeId equals de.id
                                                  where de.id == 2 || de.id == 9 || de.id == 10
                                                  select new MpharmacySpec
                                                  {
                                                      MPharmacyspecid = s.id,
                                                      MPharmacyspecname = s.specializationName
                                                  }).ToList();

            ViewBag.MPharmacyFacultyspec = MPharmacyFacultySpecialization;

            ViewBag.PGSpecializations = DepartmentsData.Where(e => e.DegreeTypeId == 2).Select(e => new { Specid = e.SpecializationId, Specname = e.Specialization }).ToList();

            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddComplianceFaculty(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber && r.Blacklistfaculy == false).Select(r => r).FirstOrDefault();
            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.Where(a => a.academicYearId == prAy).ToList();
            var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
            string[] RegistrationNumber = jntuh_appeal_faculty.Select(i => i.RegistrationNumber).ToArray();

            //Geeting Appeal Added Faculty
             var jntuh_appeal_facultycheck = db.jntuh_appeal_faculty_registered.Where(c => c.collegeId == userCollegeID && c.AppealReverificationSupportingDocument == null && c.academicYearId == prAy).Select(s => s).ToList();
             List<collegeappealdeptfaculty> collegeappealdeptfacultylist = new List<collegeappealdeptfaculty>();
            foreach (var item1 in jntuh_appeal_facultycheck)
            {
                collegeappealdeptfaculty collegeappealdeptfaculty =
                    new collegeappealdeptfaculty();
                collegeappealdeptfaculty.Departmentname =
                    jntuh_deparment.Where(d => d.id == item1.DepartmentId)
                        .Select(s => s.departmentName)
                        .FirstOrDefault();
                collegeappealdeptfaculty.DegreeName =
                    db.jntuh_degree.Where(d => d.id == item1.DegreeId).Select(s => s.degree).FirstOrDefault();
                collegeappealdeptfaculty.SpecializationName =
                    db.jntuh_specialization.Where(s => s.id == item1.SpecializationId)
                        .Select(s => s.specializationName)
                        .FirstOrDefault();
                collegeappealdeptfaculty.RegistrationNumber = item1.RegistrationNumber;
                collegeappealdeptfacultylist.Add(collegeappealdeptfaculty);
            }
            string facultydepartment =jntuh_deparment.Where(d => d.id == faculty.facultyDepartmentId)
                            .Select(s => s.departmentName)
                            .FirstOrDefault();
            int aadappealcount = collegeappealdeptfacultylist.Where(d => d.Departmentname == facultydepartment).Count();
            
            var degreeid = jntuh_deparment.Where(i => i.id == faculty.facultyDepartmentId).Select(s=>s.degreeId).FirstOrDefault();

            if (degreeid == 5 || degreeid == 2 || degreeid == 9 || degreeid == 10)
            {
                if (aadappealcount == 4)
                {
                    TempData["Error"] = "Appeal Submitted for this Department.";
                    return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal",
                        new
                        {
                            collegeId =
                                UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(),
                                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                        });
                }
            }
            else
            {
                if (aadappealcount == 2)
                {
                    TempData["Error"] = "Appeal Submitted for this Department.";
                    return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal",
                        new
                        {
                            collegeId =
                                UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(),
                                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                        });
                }
            }
            if (isRegisteredFaculty == null)
            {
                TempData["Error"] = "Invalid Faculty Registration Number.";
                return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (isExistingFaculty != null)
            {
                if (userCollegeID == isExistingFaculty.collegeId)
                {
                    TempData["Error"] = "Faculty is already working in your college";
                    return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
                if (userCollegeID != isExistingFaculty.collegeId)
                {
                    TempData["Error"] = "Faculty is already working in other JNTUH affiliated college.";
                    return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
            }
            #region This condition for PHD Faculty Adding  in Appeal
            //int phdid =
            //    db.jntuh_registered_faculty_education.Where(e => e.educationId == 6 && e.facultyId == isRegisteredFaculty.id)
            //        .Select(s => s.educationId)
            //        .FirstOrDefault();
            //int phdfacultycount = 0;
            //if (phdid!=0)
            //{
            //    phdfacultycount = 1;
            //    var appealregs =
            //        jntuh_appeal_faculty.Where(a => a.collegeId == userCollegeID && a.academicYearId == prAy)
            //            .Select(s => s.RegistrationNumber)
            //            .ToArray();
            //    var facultydata =
            //        db.jntuh_registered_faculty.Where(r => appealregs.Contains(r.RegistrationNumber))
            //            .Select(s => s.id)
            //            .ToArray();

            //    foreach (var itemreg in facultydata)
            //    {
            //        int fid =
            //            db.jntuh_registered_faculty_education.Where(e => e.educationId == 6 && e.facultyId == itemreg)
            //                .Select(e => e.educationId)
            //                .FirstOrDefault();
            //        if (fid==6)
            //        {
            //            phdfacultycount++;
            //        }
            //    }
            //}
            //if (phdfacultycount > 1)
            //{
            //    TempData["Error"] = "You can add only one PHD in appeal.";
            //    return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            //}
            #endregion
            var notificationPath = "~/Content/Upload/OnlineAppealDocuments/Faculty/NotificationsReports";
            var selectioncommitteePath = "~/Content/Upload/OnlineAppealDocuments/Faculty/SelectionCommitteeReports";
            var appointmentorderPath = "~/Content/Upload/OnlineAppealDocuments/Faculty/AppointmentOrders";
            var joiningreportpath = "~/Content/Upload/OnlineAppealDocuments/Faculty/JoiningReports";
            var appealReverificationScreenshot = "~/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationScreenshot";
            var AppealFacultyAadhaarDocuments = "~/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments";

            if (faculty.facultyDepartmentId != null)
            {
                var deparment = jntuh_deparment.FirstOrDefault(i => i.id == faculty.facultyDepartmentId);
                if (deparment != null)
                {
                    var jntuh_departmentcount =
                    jntuh_appeal_faculty.Where(
                        i =>
                            i.DepartmentId == faculty.facultyDepartmentId && i.SpecializationId == faculty.SpecializationId &&
                            i.DegreeId == deparment.degreeId).ToList();

                    if (faculty.Facultydeficencycount > jntuh_departmentcount.Count)
                    {

                    }
                }
            }
            else
            {
                var deparment = jntuh_deparment.FirstOrDefault(i => i.id == faculty.facultyPGDepartmentId);
                if (deparment != null)
                {
                    var jntuh_departmentcount =
                    jntuh_appeal_faculty.Where(
                        i =>
                            i.DepartmentId == faculty.facultyDepartmentId && i.SpecializationId == faculty.SpecializationId &&
                            i.DegreeId == deparment.degreeId).ToList();

                    if (faculty.Facultydeficencycount > jntuh_departmentcount.Count)
                    {

                    }
                }
            }


            if (!RegistrationNumber.Contains(faculty.FacultyRegistrationNumber.Trim()))
            {
                //int FacultyId = db.jntuh_college_faculty_registered.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(F => F.id).FirstOrDefault();
                //jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(i => i).FirstOrDefault();

                jntuh_appeal_faculty_registered UpdatedFaculty = new jntuh_appeal_faculty_registered();
                UpdatedFaculty.collegeId = isRegisteredFaculty.collegeId != null
                    ? (int)isRegisteredFaculty.collegeId
                    : 0;
                UpdatedFaculty.collegeId = faculty.collegeId;
                UpdatedFaculty.academicYearId = prAy;
                UpdatedFaculty.RegistrationNumber = faculty.FacultyRegistrationNumber.Trim();
                UpdatedFaculty.existingFacultyId = isRegisteredFaculty.id;
                UpdatedFaculty.IdentifiedFor = faculty.facultyRecruitedFor;
                //UpdatedFaculty.DepartmentId = faculty.facultyDepartmentId;
                //UpdatedFaculty.SpecializationId = faculty.SpecializationId;

                if (UpdatedFaculty.IdentifiedFor == "UG")
                {
                    UpdatedFaculty.DepartmentId = faculty.facultyDepartmentId;
                    UpdatedFaculty.SpecializationId = null;
                }
                else
                {
                    UpdatedFaculty.DepartmentId = faculty.facultyPGDepartmentId;
                    UpdatedFaculty.SpecializationId = faculty.SpecializationId;
                }


                var jntuhDepartment = jntuh_deparment.Where(i => i.id == UpdatedFaculty.DepartmentId).FirstOrDefault();
                if (jntuhDepartment != null)
                {
                    UpdatedFaculty.DegreeId = jntuhDepartment.degreeId;
                }
                //New Code Aadhaar Document
                UpdatedFaculty.AadhaarNumber = faculty.facultyAadhaarNumber != null ? faculty.facultyAadhaarNumber.Trim() : faculty.facultyAadhaarNumber;
                const int DelayOnRetry = 3000;
                try
                {
                    if (faculty.facultyAadharPhotoDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(AppealFacultyAadhaarDocuments)))
                        {
                            Directory.CreateDirectory(Server.MapPath(AppealFacultyAadhaarDocuments));
                        }
                        var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);
                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(AppealFacultyAadhaarDocuments),
                                fileName, ext));
                            UpdatedFaculty.AadhaarDocument = string.Format("{0}/{1}{2}", AppealFacultyAadhaarDocuments,
                                fileName, ext);
                        }
                    }

                    if (faculty.NotificationDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(notificationPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(notificationPath));
                        }

                        var ext = Path.GetExtension(faculty.NotificationDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.NotificationDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(notificationPath),
                                fileName, ext));
                            UpdatedFaculty.NOtificationReport = string.Format("{0}/{1}{2}", notificationPath, fileName,
                                ext);
                        }
                    }
                    if (faculty.SelectionCommitteeDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(selectioncommitteePath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(selectioncommitteePath));
                        }

                        var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(selectioncommitteePath), fileName, ext));
                            UpdatedFaculty.SelectionCommiteMinutes = string.Format("{0}/{1}{2}", selectioncommitteePath,
                                fileName, ext);
                        }
                    }
                    if (faculty.AppointmentOrderDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(appointmentorderPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(appointmentorderPath));
                        }

                        var ext = Path.GetExtension(faculty.AppointmentOrderDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.AppointmentOrderDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(appointmentorderPath), fileName, ext));
                            UpdatedFaculty.AppointMentOrder = string.Format("{0}/{1}{2}", appointmentorderPath, fileName,
                                ext);
                        }
                    }
                    if (faculty.JoiningReportDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(joiningreportpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(joiningreportpath));
                        }

                        var ext = Path.GetExtension(faculty.JoiningReportDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.JoiningReportDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(joiningreportpath), fileName, ext));
                            UpdatedFaculty.JoiningOrder = string.Format("{0}/{1}{2}", joiningreportpath, fileName, ext);
                        }
                    }

                    if (faculty.AppealReverificationScreenShot != null)
                    {
                        if (!Directory.Exists(Server.MapPath(appealReverificationScreenshot)))
                        {
                            Directory.CreateDirectory(Server.MapPath(appealReverificationScreenshot));
                        }

                        var ext = Path.GetExtension(faculty.AppealReverificationScreenShot.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName =
                                db.jntuh_college.Where(c => c.id == userCollegeID)
                                    .Select(c => c.collegeCode)
                                    .FirstOrDefault() + "_" +
                                faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            faculty.AppealReverificationScreenShot.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(appealReverificationScreenshot), fileName, ext));
                            UpdatedFaculty.AppealReverificationScreenshot = string.Format("{0}/{1}{2}",
                                appealReverificationScreenshot, fileName, ext);
                        }
                    }
                }
                catch (IOException e)
                {
                    Thread.Sleep(DelayOnRetry);
                }
                UpdatedFaculty.createdOn = DateTime.Now;
                UpdatedFaculty.createdBy = userID;
                db.jntuh_appeal_faculty_registered.Add(UpdatedFaculty);
                db.SaveChanges();

                //This code is written for certificates missing at the time of Complaince Faculty adding written by Naryana Reddy
                var checkregdata =
                    db.jntuh_appeal_faculty_registered.Where(
                        r =>
                            r.RegistrationNumber ==  faculty.FacultyRegistrationNumber.Trim() && r.academicYearId == prAy &&
                            r.AppealReverificationScreenshot != null && r.JoiningOrder != null &&
                            r.AppointMentOrder != null && r.SelectionCommiteMinutes != null &&
                            r.NOtificationReport != null && r.AadhaarDocument != null).Select(s => s).FirstOrDefault();
                if (checkregdata==null)
                {
                    var removereg =
                    db.jntuh_appeal_faculty_registered.Where(
                        r =>
                            r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim() && r.academicYearId == prAy).Select(s => s).FirstOrDefault();
                    if (removereg!=null)
                    {
                        db.jntuh_appeal_faculty_registered.Remove(removereg);
                        db.SaveChanges();
                        TempData["Success"] = null;
                        TempData["Error"] = "Faculty Registration Number Adding failed.";
                    }
                }
                else
                {
                    TempData["Success"] = "Faculty Registration Number Successfully Added.";
                    TempData["Error"] = null;
                }
               
            }
            else
            {
                TempData["Error"] = "Faculty is already appealed";
            }
            return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal");
        }

        #region Updateing Appeal Complance Faculty and Deleting Complance&Reverification Faculty
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        public ActionResult UpdateComplianceFaculty(string registrationnumber)
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            int facultyId = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (string.IsNullOrEmpty(registrationnumber))
            {
                return RedirectToAction("AppealReverificationFaculty", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }

            var specializatioName = "";
            var jntuh_degree = db.jntuh_degree.Where(e => e.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();
            var appealfacultyregistred =
                db.jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == registrationnumber.Trim())
                    .Select(s => s)
                    .FirstOrDefault();
            CollegeFaculty faculty = new CollegeFaculty();
            if (appealfacultyregistred != null)
            {
                faculty.FacultyRegistrationNumber = appealfacultyregistred.RegistrationNumber.Trim();
                faculty.facultyAadhaarNumber = appealfacultyregistred.AadhaarNumber;
                faculty.facultyAadharDocument = appealfacultyregistred.AadhaarDocument;
                faculty.ViewJoiningReportDocument = appealfacultyregistred.JoiningOrder;
                faculty.ViewNotificationDocument = appealfacultyregistred.NOtificationReport;
                faculty.ViewSelectionCommitteeDocument = appealfacultyregistred.SelectionCommiteMinutes;
                faculty.ViewAppointmentOrderDocument = appealfacultyregistred.AppointMentOrder;
                faculty.ViewAppealReverificationScreenShot = appealfacultyregistred.AppealReverificationScreenshot;
                faculty.facultyRecruitedFor = appealfacultyregistred.IdentifiedFor;

                faculty.DegreeName =
                    jntuh_degree.Where(r => r.id == appealfacultyregistred.DegreeId)
                        .Select(s => s.degree)
                        .FirstOrDefault();
                if (faculty.facultyRecruitedFor == "UG")
                {
                    faculty.facultyDepartmentId = appealfacultyregistred.DepartmentId != null
                        ? (int)appealfacultyregistred.DepartmentId
                        : 0;
                    faculty.facultyDepartmentId = faculty.facultyDepartmentId;
                    faculty.department =
                        jntuh_department.Where(e => e.id == faculty.facultyDepartmentId)
                            .Select(e => e.jntuh_degree.degree + " -" + e.departmentName)
                            .FirstOrDefault();
                }
                else
                {
                    faculty.facultyPGDepartmentId = appealfacultyregistred.DepartmentId != null
                         ? (int)appealfacultyregistred.DepartmentId
                         : 0;
                    faculty.facultyPGDepartmentId = faculty.facultyPGDepartmentId;
                    faculty.department =
                        jntuh_department.Where(e => e.id == faculty.facultyPGDepartmentId)
                            .Select(e => e.jntuh_degree.degree + " -" + e.departmentName)
                            .FirstOrDefault();

                    faculty.SpecializationId = appealfacultyregistred.SpecializationId != null
                         ? (int)appealfacultyregistred.SpecializationId
                         : 0; ;
                    faculty.SpecializationName = faculty.SpecializationId == null ? null : jntuh_specialization.Where(e => e.id == faculty.SpecializationId).Select(w => w.specializationName).FirstOrDefault();

                }
            }

            faculty.collegeId = appealfacultyregistred.collegeId;


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(s => s.actualYear == actualYear).Select(s => s.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(s => s.actualYear == (actualYear - 2)).Select(s => s.id).FirstOrDefault();

            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == faculty.collegeId && (e.academicYearId == AY1 || e.academicYearId == AY2 || e.academicYearId == AY3) && e.courseStatus != "Closure").Select(e => e).ToList();

            var SpecalizationIds = jntuh_college_intake_existing.Select(e => e.specializationId).Distinct().ToList();
            return PartialView(faculty);
        }
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult UpdateComplianceFaculty(CollegeFaculty faculty)
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber && r.Blacklistfaculy == false).Select(r => r).FirstOrDefault();
            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.Where(a => a.academicYearId == prAy).ToList();
            var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
            string[] RegistrationNumber = jntuh_appeal_faculty.Select(i => i.RegistrationNumber).ToArray();

            //Geeting Appeal Added Faculty


            if (faculty.FacultyRegistrationNumber == null || isRegisteredFaculty == null)
            {
                TempData["Error"] = "Invalid Faculty Registration Number.";
                return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (isExistingFaculty != null)
            {
                if (userCollegeID == isExistingFaculty.collegeId)
                {
                    TempData["Error"] = "Faculty is already working in your college";
                    return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
                if (userCollegeID != isExistingFaculty.collegeId)
                {
                    TempData["Error"] = "Faculty is already working in other JNTUH affiliated college.";
                    return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
            }
            #region This condition for PHD Faculty Adding  in Appeal
            //int phdid =
            //    db.jntuh_registered_faculty_education.Where(e => e.educationId == 6 && e.facultyId == isRegisteredFaculty.id)
            //        .Select(s => s.educationId)
            //        .FirstOrDefault();
            //int phdfacultycount = 0;
            //if (phdid!=0)
            //{
            //    phdfacultycount = 1;
            //    var appealregs =
            //        jntuh_appeal_faculty.Where(a => a.collegeId == userCollegeID && a.academicYearId == prAy)
            //            .Select(s => s.RegistrationNumber)
            //            .ToArray();
            //    var facultydata =
            //        db.jntuh_registered_faculty.Where(r => appealregs.Contains(r.RegistrationNumber))
            //            .Select(s => s.id)
            //            .ToArray();

            //    foreach (var itemreg in facultydata)
            //    {
            //        int fid =
            //            db.jntuh_registered_faculty_education.Where(e => e.educationId == 6 && e.facultyId == itemreg)
            //                .Select(e => e.educationId)
            //                .FirstOrDefault();
            //        if (fid==6)
            //        {
            //            phdfacultycount++;
            //        }
            //    }
            //}
            //if (phdfacultycount > 1)
            //{
            //    TempData["Error"] = "You can add only one PHD in appeal.";
            //    return RedirectToAction("AppealCollegeFacultyWithIntake", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            //}
            #endregion
            var notificationPath = "~/Content/Upload/OnlineAppealDocuments/Faculty/NotificationsReports";
            var selectioncommitteePath = "~/Content/Upload/OnlineAppealDocuments/Faculty/SelectionCommitteeReports";
            var appointmentorderPath = "~/Content/Upload/OnlineAppealDocuments/Faculty/AppointmentOrders";
            var joiningreportpath = "~/Content/Upload/OnlineAppealDocuments/Faculty/JoiningReports";
            var appealReverificationScreenshot = "~/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationScreenshot";
            var AppealFacultyAadhaarDocuments = "~/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments";


            //int FacultyId = db.jntuh_college_faculty_registered.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(F => F.id).FirstOrDefault();
            //jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(i => i).FirstOrDefault();

            jntuh_appeal_faculty_registered UpdatedFaculty = db.jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim()).FirstOrDefault();

            //New Code Aadhaar Document
            UpdatedFaculty.AadhaarNumber = faculty.facultyAadhaarNumber != null ? faculty.facultyAadhaarNumber.Trim() : faculty.facultyAadhaarNumber;
            const int DelayOnRetry = 3000;
            try
            {
                if (faculty.facultyAadharPhotoDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(AppealFacultyAadhaarDocuments)))
                    {
                        Directory.CreateDirectory(Server.MapPath(AppealFacultyAadhaarDocuments));
                    }
                    var ext = Path.GetExtension(faculty.facultyAadharPhotoDocument.FileName);
                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.facultyAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(AppealFacultyAadhaarDocuments),
                            fileName, ext));
                        UpdatedFaculty.AadhaarDocument = string.Format("{0}/{1}{2}", AppealFacultyAadhaarDocuments,
                            fileName, ext);
                    }
                }

                if (faculty.NotificationDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(notificationPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(notificationPath));
                    }

                    var ext = Path.GetExtension(faculty.NotificationDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.NotificationDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(notificationPath),
                            fileName, ext));
                        UpdatedFaculty.NOtificationReport = string.Format("{0}/{1}{2}", notificationPath, fileName,
                            ext);
                    }
                }
                if (faculty.SelectionCommitteeDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(selectioncommitteePath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(selectioncommitteePath));
                    }

                    var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(selectioncommitteePath), fileName, ext));
                        UpdatedFaculty.SelectionCommiteMinutes = string.Format("{0}/{1}{2}", selectioncommitteePath,
                            fileName, ext);
                    }
                }
                if (faculty.AppointmentOrderDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(appointmentorderPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(appointmentorderPath));
                    }

                    var ext = Path.GetExtension(faculty.AppointmentOrderDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.AppointmentOrderDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(appointmentorderPath), fileName, ext));
                        UpdatedFaculty.AppointMentOrder = string.Format("{0}/{1}{2}", appointmentorderPath, fileName,
                            ext);
                    }
                }
                if (faculty.JoiningReportDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(joiningreportpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(joiningreportpath));
                    }

                    var ext = Path.GetExtension(faculty.JoiningReportDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.JoiningReportDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(joiningreportpath), fileName, ext));
                        UpdatedFaculty.JoiningOrder = string.Format("{0}/{1}{2}", joiningreportpath, fileName, ext);
                    }
                }

                if (faculty.AppealReverificationScreenShot != null)
                {
                    if (!Directory.Exists(Server.MapPath(appealReverificationScreenshot)))
                    {
                        Directory.CreateDirectory(Server.MapPath(appealReverificationScreenshot));
                    }

                    var ext = Path.GetExtension(faculty.AppealReverificationScreenShot.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.collegeCode)
                                .FirstOrDefault() + "_" +
                            faculty.FacultyRegistrationNumber + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        faculty.AppealReverificationScreenShot.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(appealReverificationScreenshot), fileName, ext));
                        UpdatedFaculty.AppealReverificationScreenshot = string.Format("{0}/{1}{2}",
                            appealReverificationScreenshot, fileName, ext);
                    }
                }
            }
            catch (IOException e)
            {
                Thread.Sleep(DelayOnRetry);
            }
            UpdatedFaculty.updatedOn = DateTime.Now;
            UpdatedFaculty.updatedBy = userID;
            if (UpdatedFaculty != null)
            {
                db.Entry(UpdatedFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Faculty Registration Number updated Successfully..";
                TempData["Error"] = null;
            }
            return RedirectToAction("AppealCollegeFacultyDetails", "CollegeAppeal");
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        public ActionResult DeleteAppealFaculty(string registrationnumber)
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            int facultyId = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            if (string.IsNullOrEmpty(registrationnumber))
            {
                TempData["Error"] = "No Data found.";
                return RedirectToAction("AppealReverificationFaculty", "CollegeAppeal", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                    }
                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                    //return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            jntuh_appeal_faculty_registered appealfaculty =
                db.jntuh_appeal_faculty_registered.Where(p => p.RegistrationNumber == registrationnumber.Trim()&&p.collegeId==userCollegeID&&p.academicYearId==prAy)
                    .Select(s => s)
                    .FirstOrDefault();
            if (appealfaculty!=null)
            {
                db.jntuh_appeal_faculty_registered.Remove(appealfaculty);
                db.SaveChanges();
                TempData["Success"] = "Faculty Registration Number Deleted Successfully..";
                TempData["Error"] = null;
            }
            return RedirectToAction("AppealCollegeFacultyDetails", "CollegeAppeal");
        }

        #endregion
        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult LabsForAppeal()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            List<CollegeFacultyWithIntakeReport> facultyCounts = new List<CollegeFacultyWithIntakeReport>();
            List<AnonymousLabclass> collegeLabAnonymousLabclass = new List<AnonymousLabclass>();
            var collegefaculty = new CollegeFacultyWithIntakeReport()
            {
                collegeId = collegeId
            };
            facultyCounts.Add(collegefaculty);


            #region CollegeEditStatus
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    //return RedirectToAction("CollegeDashboard", "Dashboard");
                    return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            #endregion
            #region For labs
            var jntuh_specialization = db.jntuh_specialization.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();

            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.courseStatus != "Closure" && e.academicYearId == 11 && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToArray();
            int[] DegreeIDs = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == 4 && specializationIds.Contains(l.SpecializationID)).Select(l => l.DegreeID).ToArray();

            //List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
            //                                            .Where(l => specializationIds.Contains(l.SpecializationID))
            //                                            .Select(l => new Lab
            //                                            {
            //                                                EquipmentID = l.id,
            //                                                degreeId = l.DegreeID,
            //                                                degree = l.jntuh_degree.degree,
            //                                                degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
            //                                                departmentId = l.DepartmentID,
            //                                                department = l.jntuh_department.departmentName,
            //                                                specializationId = l.SpecializationID,
            //                                                specializationName = l.jntuh_specialization.specializationName,
            //                                                year = l.Year,
            //                                                Semester = l.Semester,
            //                                                Labcode = l.Labcode,
            //                                                LabName = l.LabName,
            //                                                EquipmentName = l.EquipmentName
            //                                            })
            //                                            .OrderBy(l => l.degreeDisplayOrder)
            //                                            .ThenBy(l => l.department)
            //                                            .ThenBy(l => l.specializationName)
            //                                            .ThenBy(l => l.year).ThenBy(l => l.Semester)
            //                                            .ToList();


            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            // var jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().ToList();
            List<jntuh_lab_master> jntuh_lab_masters = new List<jntuh_lab_master>();

            if (CollegeAffiliationStatus == "Yes")
            {
                if (DegreeIDs.Contains(4))
                {
                    jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => (l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.CollegeId == collegeId).ToList();
                }
                else
                {
                    jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == collegeId && !l.EquipmentName.Contains("desirable")).ToList();
                }

            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {

                if (DegreeIDs.Contains(4))
                {
                    if (specializationIds.Contains(33) || specializationIds.Contains(43))
                    {
                        jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => (l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.CollegeId == null && !l.EquipmentName.Contains("desirable")).ToList();
                    }
                    else
                    {
                        jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => (l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.CollegeId == null && l.Labcode != "PH105BS" && !l.EquipmentName.Contains("desirable")).ToList();
                    }
                }
                else
                {
                    jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == null && !l.EquipmentName.Contains("desirable")).ToList();
                }
            }



            //if (DegreeIDs.Contains(4))
            //{
            //    jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => (l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.CollegeId == collegeId).ToList();
            //}
            //else
            //{
            //    jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().Where(l => (l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.CollegeId == null).ToList();
            //}



            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabAnonymousLabclass = jntuh_lab_masters.Select(l => new AnonymousLabclass
                {
                    id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                    EquipmentID = l.id,
                    Department = l.jntuh_department.departmentName,
                    LabName = l.LabName,
                    EquipmentName = l.EquipmentName,
                    LabCode = l.Labcode,
                    year = l.Year,
                    Semester = l.Semester,
                    specializationId = l.SpecializationID
                    // DepartmentId=(int)jntuh_specialization.Where(e=>e.id==l.SpecializationID).Select(e=>e.departmentId).FirstOrDefault(),
                    //Department = jntuh_department.Where(t => t.id == DepartmentId).Select(t => t.departmentName).FirstOrDefault(),
                })
                                                     .OrderBy(l => l.LabName)
                                                     .ThenBy(l => l.EquipmentName)
                                                     .ToList();

            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {

                collegeLabAnonymousLabclass = jntuh_lab_masters.Select(l => new AnonymousLabclass
                {
                    id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                    EquipmentID = l.id,
                    Department = l.jntuh_department.departmentName,
                    LabName = l.LabName,
                    EquipmentName = l.EquipmentName,
                    LabCode = l.Labcode,
                    year = l.Year,
                    Semester = l.Semester,
                    specializationId = l.SpecializationID
                })
                                                   .OrderBy(l => l.LabName)
                                                   .ThenBy(l => l.EquipmentName)
                                                   .ToList();
            }

            // var collegeEquipments = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeId).Select(l => l.EquipmentID).Distinct().ToArray();

            //var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { EquipmentID = c.id, LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
            //                           .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();
            //list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

            // var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeId && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

            //foreach (var coll in collegeLabAnonymousLabclass)
            //{
            //    var deptname = jntuh_specialization.FirstOrDefault(i => i.id == coll.specializationId).jntuh_department.departmentName;
            //    coll.Department = deptname;
            //}


            //collegeLabAnonymousLabclass = collegeLabAnonymousLabclass.ToList().ForEach(i => i.DepartmentId = jntuh_specialization.FirstOrDefault(l => l.id == i.specializationId).departmentId);

            var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();


            //  list1 = list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

            #region this code written by suresh

            int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

            int[] clgequipmentIDs = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeId && labequipmentIds.Contains(l.EquipmentID) && l.isActive == true).Select(i => i.EquipmentID).ToArray();

            list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID)).ToList();


            #endregion
            if (facultyCounts.Count > 0)
            {
                facultyCounts.FirstOrDefault().LabsListDefs1 = list1.ToList();
                //facultyCounts.FirstOrDefault().LabsListDefs = list1;
            }

            #endregion
            List<AnonymousMBAMACclass> MBAMACDetails = new List<AnonymousMBAMACclass>();
            if (collegeId != null)
            {
                //Commented on 18-06-2018 by Narayana Reddy
                //var mbadef = db.jntuh_appeal_mbadeficiency.FirstOrDefault(i => i.CollegeId == collegeId);
                //if (mbadef != null)
                //{
                var colcode = db.jntuh_college.FirstOrDefault(i => i.id == collegeId).collegeCode;
                var macadreess = new AnonymousMBAMACclass();
                macadreess.CollegeId = collegeId;
                macadreess.CollegeCode = colcode;
                //macadreess.ComputerDeficiencyCount = mbadef.ComputersDeficencyCount;
                //macadreess.id = mbadef.Id;
                MBAMACDetails.Add(macadreess);
                facultyCounts.FirstOrDefault().MBAMACDetails = MBAMACDetails;
                //}

                int[] collegeids = new int[] { 343, 13, 101, 67, 394 };
                if (collegeids.Contains(collegeId))
                {
                    ViewBag.Ismbaclg = true;
                }
                else
                {
                    ViewBag.Ismbaclg = false;
                }

            }
            return View(facultyCounts);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult AddEditRecord(int? id, string collegeId, int? eqpid, int? eqpno)
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
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_laboratories.Where(i => i.id == id).Select(i => i.CollegeID).FirstOrDefault();
                    }
                }
            }

            Lab laboratories = new Lab();
            laboratories.collegeId = userCollegeID;

            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master
                                    join labs in db.jntuh_appeal_college_laboratories on m.id equals labs.EquipmentID
                                    where (labs.CollegeID == userCollegeID && labs.id == id)
                                    select new Lab
                                    {
                                        id = labs.id,
                                        collegeId = userCollegeID,
                                        EquipmentID = labs.EquipmentID,
                                        EquipmentName = m.EquipmentName,
                                        LabEquipmentName = labs.EquipmentName,
                                        EquipmentNo = labs.EquipmentNo,
                                        Make = labs.Make,
                                        Model = labs.Model,
                                        EquipmentUniqueID = labs.EquipmentUniqueID,
                                        AvailableUnits = labs.AvailableUnits,
                                        AvailableArea = labs.AvailableArea,
                                        RoomNumber = labs.RoomNumber,
                                        createdBy = labs.createdBy,
                                        createdOn = labs.createdOn,
                                        IsActive = true,
                                        degreeId = m.DegreeID,
                                        departmentId = m.DepartmentID,
                                        specializationId = m.SpecializationID,
                                        degree = m.jntuh_degree.degree,
                                        department = m.jntuh_department.departmentName,
                                        specializationName = m.jntuh_specialization.specializationName,
                                        year = m.Year,
                                        Semester = m.Semester,
                                        Labcode = m.Labcode,
                                        LabName = m.LabName,
                                        EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                        // EquipmentDateOfPurchasing1 = labs.EquipmentDateOfPurchasing != null ? string.Format("{0:yyyy-MM-dd}", labs.EquipmentDateOfPurchasing.Value) : null
                                        //,
                                        DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                        ViewEquipmentPhoto = labs.EquipmentPhoto,
                                        ViewDelivaryChalanaImage = labs.DelivaryChalanaImage,
                                        ViewBankStatementImage = labs.BankStatementImage,
                                        ViewStockRegisterEntryImage = labs.StockRegisterEntryImage,
                                        ViewReVerificationScreenImage = labs.ReVerificationScreenShot
                                        // AffiliationStatus=labs.

                                    }).FirstOrDefault();
                    if (laboratories != null)
                    {
                        laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                        laboratories.DelivaryChalanaDate1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                        return PartialView("_LaboratoriesData", laboratories);
                    }
                    else
                    {
                        eqpid = id;
                        ViewBag.IsUpdate = false;
                        jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                        laboratories.collegeId = userCollegeID;
                        laboratories.degreeId = master.DegreeID;
                        laboratories.degree = master.jntuh_degree.degree;
                        laboratories.departmentId = master.DepartmentID;
                        laboratories.department = master.jntuh_department.departmentName;
                        laboratories.specializationId = master.SpecializationID;
                        laboratories.specializationName = master.jntuh_specialization.specializationName;
                        laboratories.year = master.Year;
                        laboratories.LabName = master.LabName;
                        laboratories.EquipmentName = master.EquipmentName;
                        laboratories.EquipmentNo = eqpno;
                        string EIds = master.ExperimentsIds;
                        if (EIds != null && EIds != "")
                            laboratories.EquipmentIds = EIds.Remove(EIds.Length - 1, 1);
                        laboratories.NoofUnits = master.noofUnits;
                        //int? eqno = db.jntuh_college_laboratories.Where(l => l.EquipmentID == eqpid && l.CollegeID == userCollegeID).OrderByDescending(l => l.EquipmentNo).Select(l => l.EquipmentNo).FirstOrDefault();
                        //if (eqno != null)
                        //{
                        //    laboratories.EquipmentNo = eqno + 1;
                        //}
                        //else
                        //{
                        //    laboratories.EquipmentNo = 1;
                        //}
                        laboratories.EquipmentID = master.id;
                        laboratories.Semester = master.Semester;
                        laboratories.Labcode = master.Labcode;

                        return PartialView("_LaboratoriesData", laboratories);
                    }

                }
                else
                {
                    ViewBag.IsUpdate = false;
                    jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                    laboratories.collegeId = userCollegeID;
                    laboratories.degreeId = master.DegreeID;
                    laboratories.degree = master.jntuh_degree.degree;
                    laboratories.departmentId = master.DepartmentID;
                    laboratories.department = master.jntuh_department.departmentName;
                    laboratories.specializationId = master.SpecializationID;
                    laboratories.specializationName = master.jntuh_specialization.specializationName;
                    laboratories.year = master.Year;
                    laboratories.LabName = master.LabName;
                    laboratories.EquipmentName = master.EquipmentName;
                    laboratories.EquipmentNo = eqpno;
                    string EIds = master.ExperimentsIds;
                    if (EIds != null && EIds != "")
                        laboratories.EquipmentIds = EIds.Remove(EIds.Length - 1, 1);
                    laboratories.NoofUnits = master.noofUnits;
                    //int? eqno = db.jntuh_college_laboratories.Where(l => l.EquipmentID == eqpid && l.CollegeID == userCollegeID).OrderByDescending(l => l.EquipmentNo).Select(l => l.EquipmentNo).FirstOrDefault();
                    //if (eqno != null)
                    //{
                    //    laboratories.EquipmentNo = eqno + 1;
                    //}
                    //else
                    //{
                    //    laboratories.EquipmentNo = 1;
                    //}
                    laboratories.EquipmentID = master.id;
                    laboratories.Semester = master.Semester;
                    laboratories.Labcode = master.Labcode;

                    return PartialView("_LaboratoriesData", laboratories);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master
                                    join labs in db.jntuh_college_laboratories on m.id equals labs.EquipmentID
                                    where (labs.CollegeID == userCollegeID && labs.id == id)
                                    select new Lab
                                    {
                                        id = labs.id,
                                        collegeId = userCollegeID,
                                        EquipmentID = labs.EquipmentID,
                                        EquipmentName = m.EquipmentName,
                                        LabEquipmentName = labs.EquipmentName,
                                        EquipmentNo = labs.EquipmentNo,
                                        Make = labs.Make,
                                        Model = labs.Model,
                                        EquipmentUniqueID = labs.EquipmentUniqueID,
                                        AvailableUnits = labs.AvailableUnits,
                                        AvailableArea = labs.AvailableArea,
                                        RoomNumber = labs.RoomNumber,
                                        createdBy = labs.createdBy,
                                        createdOn = labs.createdOn,
                                        IsActive = true,

                                        degreeId = m.DegreeID,
                                        departmentId = m.DepartmentID,
                                        specializationId = m.SpecializationID,
                                        degree = m.jntuh_degree.degree,
                                        department = m.jntuh_department.departmentName,
                                        specializationName = m.jntuh_specialization.specializationName,
                                        year = m.Year,
                                        Semester = m.Semester,
                                        Labcode = m.Labcode,
                                        LabName = m.LabName,
                                        EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                        DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                        ViewEquipmentPhoto = labs.EquipmentPhoto
                                        // EquipmentPhoto  = labs.EquipmentPhoto
                                    }).FirstOrDefault();
                    laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                    laboratories.DelivaryChalanaDate1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                }
                else
                {
                    ViewBag.IsUpdate = false;
                    jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                    laboratories.collegeId = userCollegeID;
                    laboratories.degreeId = master.DegreeID;
                    laboratories.degree = master.jntuh_degree.degree;
                    laboratories.departmentId = master.DepartmentID;
                    laboratories.department = master.jntuh_department.departmentName;
                    laboratories.specializationId = master.SpecializationID;
                    laboratories.specializationName = master.jntuh_specialization.specializationName;
                    laboratories.year = master.Year;
                    laboratories.LabName = master.LabName;
                    laboratories.EquipmentName = master.EquipmentName;
                    //int? eqno = db.jntuh_college_laboratories.Where(l => l.EquipmentID == eqpid && l.CollegeID == userCollegeID).OrderByDescending(l => l.EquipmentNo).Select(l => l.EquipmentNo).FirstOrDefault();
                    //if (eqno != null)
                    //{
                    //    laboratories.EquipmentNo = eqno + 1;
                    //}
                    //else
                    //{
                    //    laboratories.EquipmentNo = 1;
                    //}
                    laboratories.EquipmentID = master.id;
                    laboratories.Semester = master.Semester;
                    laboratories.Labcode = master.Labcode;
                    string EIds = master.ExperimentsIds;
                    if (EIds != null && EIds != "")
                        laboratories.EquipmentIds = EIds.Remove(EIds.Length - 1, 1);
                }

                return View("LaboratoriesData", laboratories);
            }

            return PartialView("_LaboratoriesData", laboratories);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult AddEditRecord(Lab laboratories, string cmd, int? pageNumber)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = laboratories.collegeId;
            }
            if (laboratories.EquipmentUniqueID == null)
            {
                laboratories.EquipmentUniqueID = string.Empty;
            }
            if (ModelState.IsValid)
            {

                jntuh_appeal_college_laboratories jntuh_appeal_college_laboratories = new jntuh_appeal_college_laboratories();
                jntuh_appeal_college_laboratories.CollegeID = userCollegeID;
                jntuh_appeal_college_laboratories.academicYearId = prAy;
                jntuh_appeal_college_laboratories.EquipmentID = laboratories.EquipmentID;
                jntuh_appeal_college_laboratories.LabName = laboratories.LabName;
                jntuh_appeal_college_laboratories.Make = laboratories.Make;
                jntuh_appeal_college_laboratories.Model = laboratories.Model;
                jntuh_appeal_college_laboratories.EquipmentUniqueID = laboratories.EquipmentUniqueID;
                jntuh_appeal_college_laboratories.EquipmentName = laboratories.EquipmentName;
                jntuh_appeal_college_laboratories.AvailableUnits = laboratories.AvailableUnits;
                jntuh_appeal_college_laboratories.AvailableArea = laboratories.AvailableArea;
                jntuh_appeal_college_laboratories.RoomNumber = laboratories.RoomNumber;
                jntuh_appeal_college_laboratories.EquipmentNo = laboratories.EquipmentNo;
                jntuh_appeal_college_laboratories.isActive = true;
                if (laboratories.EquipmentDateOfPurchasing1 != null)
                {
                    laboratories.EquipmentDateOfPurchasing1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing1);
                    jntuh_appeal_college_laboratories.EquipmentDateOfPurchasing = Convert.ToDateTime(laboratories.EquipmentDateOfPurchasing1);

                }

                if (laboratories.DelivaryChalanaDate1 != null)
                {
                    laboratories.DelivaryChalanaDate1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate1);
                    jntuh_appeal_college_laboratories.DelivaryChalanaDate = Convert.ToDateTime(laboratories.DelivaryChalanaDate1);
                }

                var fileName = "";
                string randID = string.Empty;
                if (laboratories.EquipmentPhoto != null)
                {
                    // int Id = db.jntuh_college_news.Count() > 0 ? db.jntuh_newsevents.Select(d => d.id).Max() : 0;
                    // Id = Id + 1;
                    // string RamdomCode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeNews.collegeId).Select(r => r.RandamCode).FirstOrDefault();                
                    // Random rnd = new Random();
                    //int RandomNumber = rnd.Next(1000, 9999);
                    //string randID = string.Empty;
                    //// RamdomCode; + Id;
                    if (!Directory.Exists(Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/EquipmentsPhotos")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/EquipmentsPhotos"));
                    }

                    var ext = Path.GetExtension(laboratories.EquipmentPhoto.FileName);
                    if (ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                    {
                        string fileName1 = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff"); //+ "-" + laboratories.EquipmentPhoto.FileName.Substring(0, 1);
                        fileName = userCollegeID + "-" + laboratories.EquipmentID + "-" + fileName1;
                        //string path = Server.MapPath("~/Content/Upload/EquipmentsPhotos/" + DateTime.Now.ToString()+fileName);
                        var PicName = userCollegeID + "-" + laboratories.EquipmentID + "-" + DateTime.Now.ToString() + "-";
                        laboratories.EquipmentPhoto.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/EquipmentsPhotos"), fileName, ext));

                        //jntuh_college_laboratories.EquipmentPhoto = fileName;
                        jntuh_appeal_college_laboratories.EquipmentPhoto = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else
                {
                    jntuh_appeal_college_laboratories.EquipmentPhoto = laboratories.ViewEquipmentPhoto;
                }


                //Delivery challan pdf code
                var deliverychallanpath = "~/Content/Upload/OnlineAppealDocuments/Labs/DeliverychallanDocuments";
                if (laboratories.DelivaryChalanaImage != null)
                {
                    if (!Directory.Exists(Server.MapPath(deliverychallanpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(deliverychallanpath));
                    }

                    var ext = Path.GetExtension(laboratories.DelivaryChalanaImage.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string labfileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.id)
                                .FirstOrDefault() + "_" +
                            laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        laboratories.DelivaryChalanaImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(deliverychallanpath), labfileName, ext));
                        jntuh_appeal_college_laboratories.DelivaryChalanaImage = string.Format("{0}/{1}{2}", deliverychallanpath, labfileName, ext);
                    }
                }
                else if (laboratories.ViewDelivaryChalanaImage != null)
                {
                    jntuh_appeal_college_laboratories.DelivaryChalanaImage = laboratories.ViewDelivaryChalanaImage;
                }

                //Bank statement Pdf code
                var bankstatmentpath = "~/Content/Upload/OnlineAppealDocuments/Labs/BankstatementDocuments";
                if (laboratories.BankStatementImage != null)
                {
                    if (!Directory.Exists(Server.MapPath(bankstatmentpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(bankstatmentpath));
                    }

                    var ext = Path.GetExtension(laboratories.BankStatementImage.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string labfileName1 =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.id)
                                .FirstOrDefault() + "_" +
                            laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        laboratories.BankStatementImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(bankstatmentpath), labfileName1, ext));
                        jntuh_appeal_college_laboratories.BankStatementImage = string.Format("{0}/{1}{2}", bankstatmentpath, labfileName1, ext);
                    }
                }
                else if (laboratories.ViewBankStatementImage != null)
                {
                    jntuh_appeal_college_laboratories.BankStatementImage = laboratories.ViewBankStatementImage;
                }

                //Stock Register Pdf code
                var stockregisterpath = "~/Content/Upload/OnlineAppealDocuments/Labs/StockregisterentryDocuments";
                if (laboratories.StockRegisterEntryImage != null)
                {
                    if (!Directory.Exists(Server.MapPath(stockregisterpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(stockregisterpath));
                    }

                    var ext = Path.GetExtension(laboratories.StockRegisterEntryImage.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string labfileName2 =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.id)
                                .FirstOrDefault() + "_" +
                            laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        laboratories.StockRegisterEntryImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(stockregisterpath), labfileName2, ext));
                        jntuh_appeal_college_laboratories.StockRegisterEntryImage = string.Format("{0}/{1}{2}", stockregisterpath, labfileName2, ext);
                    }
                }
                else if (laboratories.ViewStockRegisterEntryImage != null)
                {
                    jntuh_appeal_college_laboratories.StockRegisterEntryImage = laboratories.ViewStockRegisterEntryImage;
                }

                if (cmd == "Save")
                {
                    var existingID = db.jntuh_appeal_college_laboratories.Where(c => c.CollegeID == userCollegeID && c.EquipmentID == laboratories.EquipmentID && c.EquipmentNo == laboratories.EquipmentNo).Select(c => c).FirstOrDefault();

                    if (existingID == null)
                    {
                        jntuh_appeal_college_laboratories.createdBy = userID;
                        jntuh_appeal_college_laboratories.createdOn = DateTime.Now;
                        db.jntuh_appeal_college_laboratories.Add(jntuh_appeal_college_laboratories);
                        db.SaveChanges();
                        TempData["Success"] = "Lab Added Successfully.";
                    }
                    else
                    {
                        TempData["Success"] = "Lab already exists.";
                    }
                }
                else
                {
                    if (laboratories.id == null || laboratories.id==0)
                    {
                        TempData["Error"] = "No Data found Lab Updated failed..";
                        return RedirectToAction("LabsForAppeal");
                    }
                    jntuh_appeal_college_laboratories.id = (int)laboratories.id;
                    jntuh_appeal_college_laboratories.createdBy = laboratories.createdBy;
                    jntuh_appeal_college_laboratories.createdOn = laboratories.createdOn;
                    jntuh_appeal_college_laboratories.updatedBy = userID;
                    jntuh_appeal_college_laboratories.updatedOn = DateTime.Now;
                    jntuh_appeal_college_laboratories.isActive = true;
                    jntuh_appeal_college_laboratories.ReVerificationScreenShot =
                        db.jntuh_appeal_college_laboratories.Where(i => i.id == (int)laboratories.id)
                            .Select(i => i.ReVerificationScreenShot)
                            .FirstOrDefault();
                    db.Entry(jntuh_appeal_college_laboratories).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Lab Updated Successfully.";
                }
            }
            int? pageNo = null;
            if (Request.Params["pageNumber"] != null)
            {
                pageNo = Convert.ToInt32(Request.QueryString["pageNumber"].ToString());
            }
            return RedirectToAction("LabsForAppeal");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult AddEditRecordReverification(int? id, string collegeId, int? eqpid, int? eqpno)
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
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_laboratories.Where(i => i.id == id).Select(i => i.CollegeID).FirstOrDefault();
                    }
                }
            }

            Lab laboratories = new Lab();
            laboratories.collegeId = userCollegeID;

            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master
                                    join labs in db.jntuh_appeal_college_laboratories on m.id equals labs.EquipmentID
                                    where (labs.CollegeID == userCollegeID && labs.id == id)
                                    select new Lab
                                    {
                                        id = labs.id,
                                        collegeId = userCollegeID,
                                        EquipmentID = labs.EquipmentID,
                                        EquipmentName = m.EquipmentName,
                                        LabEquipmentName = labs.EquipmentName,
                                        EquipmentNo = labs.EquipmentNo,
                                        Make = labs.Make,
                                        Model = labs.Model,
                                        EquipmentUniqueID = labs.EquipmentUniqueID,
                                        AvailableUnits = labs.AvailableUnits,
                                        AvailableArea = labs.AvailableArea,
                                        RoomNumber = labs.RoomNumber,
                                        createdBy = labs.createdBy,
                                        createdOn = labs.createdOn,
                                        IsActive = true,

                                        degreeId = m.DegreeID,
                                        departmentId = m.DepartmentID,
                                        specializationId = m.SpecializationID,
                                        degree = m.jntuh_degree.degree,
                                        department = m.jntuh_department.departmentName,
                                        specializationName = m.jntuh_specialization.specializationName,
                                        year = m.Year,
                                        Semester = m.Semester,
                                        Labcode = m.Labcode,
                                        LabName = m.LabName,
                                        EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                        // EquipmentDateOfPurchasing1 = labs.EquipmentDateOfPurchasing != null ? string.Format("{0:yyyy-MM-dd}", labs.EquipmentDateOfPurchasing.Value) : null
                                        //,
                                        DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                        ViewEquipmentPhoto = labs.EquipmentPhoto,
                                        ViewDelivaryChalanaImage = labs.DelivaryChalanaImage,
                                        ViewBankStatementImage = labs.BankStatementImage,
                                        ViewStockRegisterEntryImage = labs.StockRegisterEntryImage,
                                        ViewReVerificationScreenImage = labs.ReVerificationScreenShot
                                        // AffiliationStatus=labs.

                                    }).FirstOrDefault();
                    if (laboratories != null)
                    {
                        laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                        laboratories.DelivaryChalanaDate1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                        return PartialView("_LaboratoriesDataReverificationStatus", laboratories);
                    }
                    else
                    {
                        if (id==0||id==null)
                        {
                            TempData["Error"] = "try agian..";
                            return RedirectToAction("LabsForAppeal");
                        }
                        eqpid = id;
                        ViewBag.IsUpdate = false;
                        jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                        laboratories.collegeId = userCollegeID;
                        laboratories.degreeId = master.DegreeID;
                        laboratories.degree = master.jntuh_degree.degree;
                        laboratories.departmentId = master.DepartmentID;
                        laboratories.department = master.jntuh_department.departmentName;
                        laboratories.specializationId = master.SpecializationID;
                        laboratories.specializationName = master.jntuh_specialization.specializationName;
                        laboratories.year = master.Year;
                        laboratories.LabName = master.LabName;
                        laboratories.EquipmentName = master.EquipmentName;
                        laboratories.EquipmentNo = eqpno;
                        string EIds = master.ExperimentsIds;
                        if (EIds != null && EIds != "")
                            laboratories.EquipmentIds = EIds.Remove(EIds.Length - 1, 1);
                        laboratories.NoofUnits = master.noofUnits;
                        //int? eqno = db.jntuh_college_laboratories.Where(l => l.EquipmentID == eqpid && l.CollegeID == userCollegeID).OrderByDescending(l => l.EquipmentNo).Select(l => l.EquipmentNo).FirstOrDefault();
                        //if (eqno != null)
                        //{
                        //    laboratories.EquipmentNo = eqno + 1;
                        //}
                        //else
                        //{
                        //    laboratories.EquipmentNo = 1;
                        //}
                        laboratories.EquipmentID = master.id;
                        laboratories.Semester = master.Semester;
                        laboratories.Labcode = master.Labcode;

                        return PartialView("_LaboratoriesDataReverificationStatus", laboratories);
                    }

                }
                else
                {
                    ViewBag.IsUpdate = false;
                    jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                    laboratories.collegeId = userCollegeID;
                    laboratories.degreeId = master.DegreeID;
                    laboratories.degree = master.jntuh_degree.degree;
                    laboratories.departmentId = master.DepartmentID;
                    laboratories.department = master.jntuh_department.departmentName;
                    laboratories.specializationId = master.SpecializationID;
                    laboratories.specializationName = master.jntuh_specialization.specializationName;
                    laboratories.year = master.Year;
                    laboratories.LabName = master.LabName;
                    laboratories.EquipmentName = master.EquipmentName;
                    laboratories.EquipmentNo = eqpno;
                    string EIds = master.ExperimentsIds;
                    if (EIds != null && EIds != "")
                        laboratories.EquipmentIds = EIds.Remove(EIds.Length - 1, 1);
                    laboratories.NoofUnits = master.noofUnits;
                    //int? eqno = db.jntuh_college_laboratories.Where(l => l.EquipmentID == eqpid && l.CollegeID == userCollegeID).OrderByDescending(l => l.EquipmentNo).Select(l => l.EquipmentNo).FirstOrDefault();
                    //if (eqno != null)
                    //{
                    //    laboratories.EquipmentNo = eqno + 1;
                    //}
                    //else
                    //{
                    //    laboratories.EquipmentNo = 1;
                    //}
                    laboratories.EquipmentID = master.id;
                    laboratories.Semester = master.Semester;
                    laboratories.Labcode = master.Labcode;

                    return PartialView("_LaboratoriesDataReverificationStatus", laboratories);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master
                                    join labs in db.jntuh_college_laboratories on m.id equals labs.EquipmentID
                                    where (labs.CollegeID == userCollegeID && labs.id == id)
                                    select new Lab
                                    {
                                        id = labs.id,
                                        collegeId = userCollegeID,
                                        EquipmentID = labs.EquipmentID,
                                        EquipmentName = m.EquipmentName,
                                        LabEquipmentName = labs.EquipmentName,
                                        EquipmentNo = labs.EquipmentNo,
                                        Make = labs.Make,
                                        Model = labs.Model,
                                        EquipmentUniqueID = labs.EquipmentUniqueID,
                                        AvailableUnits = labs.AvailableUnits,
                                        AvailableArea = labs.AvailableArea,
                                        RoomNumber = labs.RoomNumber,
                                        createdBy = labs.createdBy,
                                        createdOn = labs.createdOn,
                                        IsActive = true,

                                        degreeId = m.DegreeID,
                                        departmentId = m.DepartmentID,
                                        specializationId = m.SpecializationID,
                                        degree = m.jntuh_degree.degree,
                                        department = m.jntuh_department.departmentName,
                                        specializationName = m.jntuh_specialization.specializationName,
                                        year = m.Year,
                                        Semester = m.Semester,
                                        Labcode = m.Labcode,
                                        LabName = m.LabName,
                                        EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                        DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                        ViewEquipmentPhoto = labs.EquipmentPhoto
                                        // EquipmentPhoto  = labs.EquipmentPhoto
                                    }).FirstOrDefault();
                    laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                    laboratories.DelivaryChalanaDate1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                }
                else
                {
                    ViewBag.IsUpdate = false;
                    jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                    laboratories.collegeId = userCollegeID;
                    laboratories.degreeId = master.DegreeID;
                    laboratories.degree = master.jntuh_degree.degree;
                    laboratories.departmentId = master.DepartmentID;
                    laboratories.department = master.jntuh_department.departmentName;
                    laboratories.specializationId = master.SpecializationID;
                    laboratories.specializationName = master.jntuh_specialization.specializationName;
                    laboratories.year = master.Year;
                    laboratories.LabName = master.LabName;
                    laboratories.EquipmentName = master.EquipmentName;
                    //int? eqno = db.jntuh_college_laboratories.Where(l => l.EquipmentID == eqpid && l.CollegeID == userCollegeID).OrderByDescending(l => l.EquipmentNo).Select(l => l.EquipmentNo).FirstOrDefault();
                    //if (eqno != null)
                    //{
                    //    laboratories.EquipmentNo = eqno + 1;
                    //}
                    //else
                    //{
                    //    laboratories.EquipmentNo = 1;
                    //}
                    laboratories.EquipmentID = master.id;
                    laboratories.Semester = master.Semester;
                    laboratories.Labcode = master.Labcode;
                    string EIds = master.ExperimentsIds;
                    if (EIds != null && EIds != "")
                        laboratories.EquipmentIds = EIds.Remove(EIds.Length - 1, 1);
                }

                return View("LaboratoriesData", laboratories);
            }

            return PartialView("_LaboratoriesDataReverificationStatus", laboratories);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult AddEditRecordReverification(Lab laboratories, string cmd, int? pageNumber)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = laboratories.collegeId;
            }
            if (laboratories.EquipmentUniqueID == null)
            {
                laboratories.EquipmentUniqueID = string.Empty;
            }
            if (ModelState.IsValid)
            {

                jntuh_appeal_college_laboratories jntuh_appeal_college_laboratories = new jntuh_appeal_college_laboratories();
                jntuh_appeal_college_laboratories.CollegeID = userCollegeID;
                jntuh_appeal_college_laboratories.academicYearId = prAy;
                jntuh_appeal_college_laboratories.EquipmentID = laboratories.EquipmentID;
                jntuh_appeal_college_laboratories.LabName = laboratories.LabName.Trim();
                jntuh_appeal_college_laboratories.Make = laboratories.Make.Trim();
                jntuh_appeal_college_laboratories.Model = laboratories.Model.Trim();
                jntuh_appeal_college_laboratories.EquipmentUniqueID = laboratories.EquipmentUniqueID;
                jntuh_appeal_college_laboratories.EquipmentName = laboratories.EquipmentName;
                jntuh_appeal_college_laboratories.AvailableUnits = laboratories.AvailableUnits;
                jntuh_appeal_college_laboratories.AvailableArea = laboratories.AvailableArea;
                jntuh_appeal_college_laboratories.RoomNumber = laboratories.RoomNumber;
                jntuh_appeal_college_laboratories.EquipmentNo = laboratories.EquipmentNo;
                jntuh_appeal_college_laboratories.isActive = true;
                if (laboratories.EquipmentDateOfPurchasing1 != null)
                {
                    laboratories.EquipmentDateOfPurchasing1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing1);
                    jntuh_appeal_college_laboratories.EquipmentDateOfPurchasing = Convert.ToDateTime(laboratories.EquipmentDateOfPurchasing1);

                }

                if (laboratories.DelivaryChalanaDate1 != null)
                {
                    laboratories.DelivaryChalanaDate1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate1);
                    jntuh_appeal_college_laboratories.DelivaryChalanaDate = Convert.ToDateTime(laboratories.DelivaryChalanaDate1);
                }

                var fileName = "";
                string randID = string.Empty;
                if (laboratories.EquipmentPhoto != null)
                {
                    // int Id = db.jntuh_college_news.Count() > 0 ? db.jntuh_newsevents.Select(d => d.id).Max() : 0;
                    // Id = Id + 1;
                    // string RamdomCode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeNews.collegeId).Select(r => r.RandamCode).FirstOrDefault();                
                    // Random rnd = new Random();
                    //int RandomNumber = rnd.Next(1000, 9999);
                    //string randID = string.Empty;
                    //// RamdomCode; + Id;
                    if (!Directory.Exists(Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/EquipmentsPhotos")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/EquipmentsPhotos"));
                    }

                    var ext = Path.GetExtension(laboratories.EquipmentPhoto.FileName);
                    if (ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                    {
                        string fileName1 = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");// + "-" + laboratories.EquipmentPhoto.FileName.Substring(0, 1);
                        fileName = userCollegeID + "-" + laboratories.EquipmentID + "-" + fileName1;
                        //string path = Server.MapPath("~/Content/Upload/EquipmentsPhotos/" + DateTime.Now.ToString()+fileName);
                        var PicName = userCollegeID + "-" + laboratories.EquipmentID + "-" + DateTime.Now.ToString() + "-";
                        laboratories.EquipmentPhoto.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/EquipmentsPhotos"), fileName, ext));

                        //jntuh_college_laboratories.EquipmentPhoto = fileName;
                        jntuh_appeal_college_laboratories.EquipmentPhoto = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else
                {
                    jntuh_appeal_college_laboratories.EquipmentPhoto = laboratories.ViewEquipmentPhoto;
                }


                //Delivery challan pdf code
                var deliverychallanpath = "~/Content/Upload/OnlineAppealDocuments/Labs/DeliverychallanDocuments";
                if (laboratories.DelivaryChalanaImage != null)
                {
                    if (!Directory.Exists(Server.MapPath(deliverychallanpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(deliverychallanpath));
                    }

                    var ext = Path.GetExtension(laboratories.DelivaryChalanaImage.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string labfileName =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.id)
                                .FirstOrDefault() + "_" +
                            laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        laboratories.DelivaryChalanaImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(deliverychallanpath), labfileName, ext));
                        jntuh_appeal_college_laboratories.DelivaryChalanaImage = string.Format("{0}/{1}{2}", deliverychallanpath, labfileName, ext);
                    }
                }
                else if (laboratories.ViewDelivaryChalanaImage != null)
                {
                    jntuh_appeal_college_laboratories.DelivaryChalanaImage = laboratories.ViewDelivaryChalanaImage;
                }

                //Bank statement Pdf code
                var bankstatmentpath = "~/Content/Upload/OnlineAppealDocuments/Labs/BankstatementDocuments";
                if (laboratories.BankStatementImage != null)
                {
                    if (!Directory.Exists(Server.MapPath(bankstatmentpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(bankstatmentpath));
                    }

                    var ext = Path.GetExtension(laboratories.BankStatementImage.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string labfileName1 =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.id)
                                .FirstOrDefault() + "_" +
                            laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        laboratories.BankStatementImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(bankstatmentpath), labfileName1, ext));
                        jntuh_appeal_college_laboratories.BankStatementImage = string.Format("{0}/{1}{2}", bankstatmentpath, labfileName1, ext);
                    }
                }
                else if (laboratories.ViewBankStatementImage != null)
                {
                    jntuh_appeal_college_laboratories.BankStatementImage = laboratories.ViewBankStatementImage;
                }

                //Stock Register Pdf code
                var stockregisterpath = "~/Content/Upload/OnlineAppealDocuments/Labs/StockregisterentryDocuments";
                if (laboratories.StockRegisterEntryImage != null)
                {
                    if (!Directory.Exists(Server.MapPath(stockregisterpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(stockregisterpath));
                    }

                    var ext = Path.GetExtension(laboratories.StockRegisterEntryImage.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string labfileName2 =
                            db.jntuh_college.Where(c => c.id == userCollegeID)
                                .Select(c => c.id)
                                .FirstOrDefault() + "_" +
                            laboratories.EquipmentID + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        laboratories.StockRegisterEntryImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(stockregisterpath), labfileName2, ext));
                        jntuh_appeal_college_laboratories.StockRegisterEntryImage = string.Format("{0}/{1}{2}", stockregisterpath, labfileName2, ext);
                    }
                }
                else if (laboratories.ViewStockRegisterEntryImage != null)
                {
                    jntuh_appeal_college_laboratories.StockRegisterEntryImage = laboratories.ViewStockRegisterEntryImage;
                }
                //Reverification screen shot image code
                var ReverificationfileName = "";

                if (laboratories.ReVerificationScreenImage != null)
                {

                    if (!Directory.Exists(Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/Reverificationscreenshot")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/Reverificationscreenshot"));
                    }

                    var ext = Path.GetExtension(laboratories.ReVerificationScreenImage.FileName);
                    if (ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                    {
                        string fileName1 = "Rev" + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                        ReverificationfileName = userCollegeID + "-" + laboratories.EquipmentID + "-" + fileName1;
                        //string path = Server.MapPath("~/Content/Upload/EquipmentsPhotos/" + DateTime.Now.ToString()+fileName);
                        var PicName = userCollegeID + "-" + laboratories.EquipmentID + "-" + DateTime.Now.ToString() + "-";
                        laboratories.ReVerificationScreenImage.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/OnlineAppealDocuments/Labs/Reverificationscreenshot"), ReverificationfileName, ext));

                        //jntuh_college_laboratories.EquipmentPhoto = fileName;
                        jntuh_appeal_college_laboratories.ReVerificationScreenShot = string.Format("{0}{1}", ReverificationfileName, ext);
                    }
                }
                else
                {
                    jntuh_appeal_college_laboratories.ReVerificationScreenShot = laboratories.ViewReVerificationScreenImage;
                }

                if (cmd == "Save")
                {
                    var existingID = db.jntuh_appeal_college_laboratories.Where(c => c.CollegeID == userCollegeID && c.academicYearId == prAy && c.EquipmentID == laboratories.EquipmentID && c.EquipmentNo == laboratories.EquipmentNo).Select(c => c).FirstOrDefault();

                    if (existingID == null)
                    {
                        jntuh_appeal_college_laboratories.createdBy = userID;
                        jntuh_appeal_college_laboratories.createdOn = DateTime.Now;
                        db.jntuh_appeal_college_laboratories.Add(jntuh_appeal_college_laboratories);
                        db.SaveChanges();
                        TempData["Success"] = "Lab Added Successfully.";
                    }
                    else
                    {
                        TempData["Success"] = "Lab already exists.";
                    }
                }
                else
                {
                    if (laboratories.id == null || laboratories.id == 0)
                    {
                        TempData["Error"] = "No Data found Lab Updated failed..";
                        return RedirectToAction("LabsForAppeal");
                    }
                    jntuh_appeal_college_laboratories.id = (int)laboratories.id;
                    jntuh_appeal_college_laboratories.createdBy = laboratories.createdBy;
                    jntuh_appeal_college_laboratories.createdOn = laboratories.createdOn;
                    jntuh_appeal_college_laboratories.updatedBy = userID;
                    jntuh_appeal_college_laboratories.updatedOn = DateTime.Now;
                    jntuh_appeal_college_laboratories.isActive = true;
                    db.Entry(jntuh_appeal_college_laboratories).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Lab Updated Successfully.";
                }
            }
            int? pageNo = null;
            if (Request.Params["pageNumber"] != null)
            {
                pageNo = Convert.ToInt32(Request.QueryString["pageNumber"].ToString());
            }
            return RedirectToAction("LabsForAppeal");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult AppealSubmission(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string CollegeCode = db.jntuh_college.Where(C => C.id == userCollegeID).Select(C => C.collegeCode).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            #region CollegeEditStatus
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);
                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    // return RedirectToAction("CollegeDashboard", "Dashboard");
                    return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }
            #endregion

            SubmitData submitData = new SubmitData();
            submitData.collegeId = userCollegeID;
            ViewBag.CollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.IscollegeEditable = db.jntuh_appeal_college_edit_status.Where(i => i.collegeId == userCollegeID).Select(i => i.IsCollegeEditable).FirstOrDefault();
            string clgCode = db.jntuh_college.Where(C => C.id == userCollegeID).Select(C => C.collegeCode).FirstOrDefault();
            var currentYear = DateTime.Now.Year;
            var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 6 && it.AcademicYearId == prAy) > 0;

            ViewBag.IsLatePaymentDone = isPaid;
            if (!isPaid)
            {
                TempData["Payment"] = "Please Pay The Appeal Fee. Colleges are requested to pay the Appeal Online Processing Fee.";
            }
            jntuh_appeal_college_edit_status jntuh_appeal_college_edit_status =
                db.jntuh_appeal_college_edit_status.Where(c => c.collegeId == userCollegeID)
                    .Select(s => s)
                    .FirstOrDefault();
            if (jntuh_appeal_college_edit_status.DeclarationPath != null)
            {
                submitData.DeclarationPathdoc = jntuh_appeal_college_edit_status.DeclarationPath;
            }
            if (jntuh_appeal_college_edit_status.FurtherAppealSupportingDocument != null)
            {
                submitData.OtherSupportingDocpath = jntuh_appeal_college_edit_status.FurtherAppealSupportingDocument;
            }
            return View(submitData);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult AppealSubmission(SubmitData submitData, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0)
            {
                userCollegeID = submitData.collegeId;
                if (userCollegeID == 0)
                {
                    return RedirectToAction("Create", "CollegeInformation");
                }
            }
            var currentDate = DateTime.Now;
            DateTime EditTODate;
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
            if (cmd == "Submit Appeal")
            {
                if (currentDate >= EditTODate)
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                else
                    SaveSubmitData(submitData);
            }
            return RedirectToAction("AppealSubmission");//, new { collegeId = userCollegeID }

        }
        private void SaveSubmitData(SubmitData submitData)
        {
            if (ModelState.IsValid)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
                string clgCode = db.jntuh_college.Where(C => C.id == userCollegeID).Select(C => C.collegeCode).FirstOrDefault();
                var currentYear = DateTime.Now.Year;
                var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 6) > 0;

                ViewBag.IsLatePaymentDone = isPaid;
                if (isPaid)
                {

                    var jntuhAppealCollegeEditStatus = db.jntuh_appeal_college_edit_status.Where(i => i.collegeId == userCollegeID).Select(i => i).FirstOrDefault();
                    if (jntuhAppealCollegeEditStatus != null)
                    {
                        //Delivery challan pdf code
                        var appealsubmissionpath = "~/Content/Upload/OnlineAppealDocuments/Appealsubmission/AppealsubmissionDocuments";
                        var othersupportingdocpath = "~/Content/Upload/OnlineAppealDocuments/Appealsubmission/OthersupportingDocument";
                        const int DelayOnRetry = 3000;
                        try
                        {                       
                        if (submitData.DeclarationPath != null)
                        {
                            if (!Directory.Exists(Server.MapPath(appealsubmissionpath)))
                            {
                                Directory.CreateDirectory(Server.MapPath(appealsubmissionpath));
                            }

                            var ext = Path.GetExtension(submitData.DeclarationPath.FileName);
                            if (ext.ToUpper().Equals(".PDF"))
                            {
                                string labfileName =
                                    db.jntuh_college.Where(c => c.id == userCollegeID)
                                        .Select(c => c.collegeCode)
                                        .FirstOrDefault() + "_" + "Appeal" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                                submitData.DeclarationPath.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(appealsubmissionpath), labfileName, ext));
                                jntuhAppealCollegeEditStatus.DeclarationPath = string.Format("{0}{1}", labfileName, ext);
                            }
                        }
                        //other supporting Document Code
                        if (submitData.OtherSupportingDoc != null)
                        {
                            if (!Directory.Exists(Server.MapPath(othersupportingdocpath)))
                            {
                                Directory.CreateDirectory(Server.MapPath(othersupportingdocpath));
                            }

                            var ext = Path.GetExtension(submitData.OtherSupportingDoc.FileName);
                            if (ext.ToUpper().Equals(".PDF"))
                            {
                                string docfileName =
                                    db.jntuh_college.Where(c => c.id == userCollegeID)
                                        .Select(c => c.collegeCode)
                                        .FirstOrDefault() + "_" + "Appeal" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                                submitData.OtherSupportingDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(othersupportingdocpath), docfileName, ext));
                                jntuhAppealCollegeEditStatus.FurtherAppealSupportingDocument = string.Format("{0}{1}", docfileName, ext);
                            }
                        }
                        }
                        catch (IOException e)
                        {
                            Thread.Sleep(DelayOnRetry);
                        }
                        jntuhAppealCollegeEditStatus.Remarks = submitData.Remarks;
                        jntuhAppealCollegeEditStatus.IsCollegeEditable = false;
                        jntuhAppealCollegeEditStatus.updatedBy = userID;
                        jntuhAppealCollegeEditStatus.updatedOn = DateTime.Now;
                        db.SaveChanges();
                        TempData["Success"] = "Appeal Submitted SuccessFully.";

                    }
                }
                else
                {
                    TempData["Payment"] = "Please Pay The Appeal Fee. Colleges are requested to pay the Fee from Online Payment Portal. Please logout from Application here and login into online payment portal for Payments.";
                }
            }
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeFacultyWithIntakeFacultyPharmacyNew()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (collegeId == 375)
                collegeId = Convert.ToInt32(WebConfigurationManager.AppSettings["PharmacyId"]);
            ViewBag.collegeId = collegeId;

            List<PharmacyReportsClass> PharmacyAppealFaculty = new List<PharmacyReportsClass>();
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.academicyearid == AY0 && C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                collegeid = Convert.ToInt32(CollegeDetails.collegeId);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {
                    if (PageEdible == false)
                    {
                        return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                    }
                }
                else
                {
                    return RedirectToAction("ViewCollegeFacultyWithIntake", "CollegeAppeal");
                }
            }
            else
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == AY0 && i.proposedIntake != 0 && i.courseStatus != "Closure").ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> collegeIntakeExistingList = new List<CollegeFacultyWithIntakeReport>();

            foreach (var item in intake)
            {
                CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                newIntake.nbaFrom = item.nbaFrom;
                newIntake.nbaTo = item.nbaTo;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization = item.jntuh_specialization.specializationName;
                newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = item.jntuh_shift.shiftName;
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.GroupBy(a => new { a.specializationId, a.shiftId }).Select(a => a.First()).ToList();

            var jntuh_college = db.jntuh_college.Where(a => a.isActive == true).Select(q => q).ToList();

            string cid = collegeId.ToString();
            var PharmacyDepartmens = new int[] { 26, 36, 27, 39, 61 };
            var FacultyData = db.jntuh_appeal_pharmacydata.Where(p => p.CollegeCode == cid).ToList();
            string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
            string PharmacyStatus = "";
            foreach (var item in collegeIntakeExisting)
            {
                if (item.Degree == "B.Pharmacy")
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                        Pharmacy.Collegeid = item.collegeId;
                        Pharmacy.Degree = item.Degree;
                        Pharmacy.DepartmentId = item.DepartmentID;
                        Pharmacy.Department = item.Department;
                        Pharmacy.SpecializationId = item.specializationId;
                        Pharmacy.Specialization = item.Specialization;
                        Pharmacy.ShiftId = item.shiftId;
                        switch (i)
                        {
                            case 1:
                                Pharmacy.PharmacySpecialization = "Group1 (Pharmaceutics , Industrial Pharmacy , Pharmaceutical Technology , 	Pharmaceutical Biotechnology , PRA)";
                                Pharmacy.GroupId = "1";
                                break;
                            case 2:
                                Pharmacy.PharmacySpecialization = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis , PAQA , QA , QAPRA , NIPER Medicinal Chemistry)";
                                Pharmacy.GroupId = "2";
                                break;
                            case 3:
                                Pharmacy.PharmacySpecialization = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice , Hospital Pharmacy , Clinical Pharmacy,  Hospital and Clinical Pharmacy)";
                                Pharmacy.GroupId = "3";
                                break;
                            default:
                                Pharmacy.PharmacySpecialization = "Group4 (Pharmacognosy, Pharmaceutical Chemistry , Phytopharmacy & Phytomedicine , NIPER  Natural Products , Pharmaceutical Biotechnology";
                                Pharmacy.GroupId = "4";
                                break;
                        }


                        Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                        Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();
                        Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).FirstOrDefault();
                        Pharmacy.NoOfAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId).Select(f => f.SpecializationWiseRequiredFaculty).FirstOrDefault();
                        Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                        {
                            Pharmacy.Deficiency = "No Deficiency";
                        }
                        else
                        {
                            Pharmacy.Deficiency = "Deficiency";
                        }

                        var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                        var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                        var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                                                         && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf).ToList();

                        var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                        int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                        Pharmacy.PHdFaculty = PhdRegNOCount;
                        Pharmacy.IsActive = false;

                        if (Pharmacy.ShiftId == 1)
                        {
                            PharmacyAppealFaculty.Add(Pharmacy);
                        }
                    }

                    PharmacyReportsClass BPharmacyObj = new PharmacyReportsClass();
                    BPharmacyObj.Collegeid = PharmacyAppealFaculty.Select(z => z.Collegeid).FirstOrDefault();
                    BPharmacyObj.Degree = "Pharmacy";
                    BPharmacyObj.DepartmentId = 26;
                    BPharmacyObj.Department = "B.Pharmacy";
                    BPharmacyObj.SpecializationId = 12;
                    BPharmacyObj.ShiftId = PharmacyAppealFaculty.Select(z => z.ShiftId).FirstOrDefault();
                    BPharmacyObj.Specialization = "B.Pharmacy";
                    BPharmacyObj.TotalIntake = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.TotalIntake).FirstOrDefault();
                    BPharmacyObj.ProposedIntake = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.ProposedIntake).FirstOrDefault();
                    BPharmacyObj.NoOfFacultyRequired = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.NoOfFacultyRequired).FirstOrDefault();
                    BPharmacyObj.NoOfAvilableFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.NoOfAvilableFaculty).FirstOrDefault();
                    BPharmacyObj.Deficiency = PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0 ? "Deficiency" : "No Deficiency";
                    BPharmacyObj.PHdFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(q => q.PHdFaculty).Sum();
                    BPharmacyObj.IsActive = true;
                    if (BPharmacyObj.ShiftId == 1)
                    {
                        PharmacyAppealFaculty.Add(BPharmacyObj);
                    }
                }
                else if (item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
                {
                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.ShiftId = item.shiftId;
                    Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                    if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                        Pharmacy.Deficiency = "Deficiency";
                    else
                        Pharmacy.Deficiency = "No Deficiency";

                    if (Pharmacy.ShiftId == 1)
                    {
                        PharmacyAppealFaculty.Add(Pharmacy);
                    }
                }
                else if (item.Degree == "M.Pharmacy")
                {
                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.ShiftId = item.shiftId;
                    Pharmacy.ProposedIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.ProposedIntake).FirstOrDefault();
                    Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();

                    Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).FirstOrDefault();
                    Pharmacy.NoOfAvilableFaculty = FacultyData.Count(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null);
                    Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.SpecializationWiseRequiredFaculty).FirstOrDefault();
                    Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();

                    var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                    var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                    var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                                                     && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf).ToList();

                    var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                    int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                    Pharmacy.PHdFaculty = PhdRegNOCount;

                    if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                    {
                        if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                            Pharmacy.Deficiency = "Deficiency";
                        else
                            Pharmacy.Deficiency = "No Deficiency";
                    }
                    else
                    {
                        Pharmacy.Deficiency = "Deficiency";
                    }
                    if (Pharmacy.ShiftId == 1)
                    {
                        PharmacyAppealFaculty.Add(Pharmacy);
                    }
                }
            }
            return View(PharmacyAppealFaculty);
        }

        //Pharm D BAS Flag Clear Faculty uploadin from colleges
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult PharmDuplaodfaculty()
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            if (Membership.GetUser() != null)
            {
                int[] pharmdcids = { 9, 24, 27, 30, 44, 47, 52, 55, 65, 90, 105, 110, 117, 127, 135, 139, 159, 169, 180, 202, 204, 206, 219, 234, 263, 267, 297, 319, 376, 379, 389, 392, 428, 442, 375 };
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0 || userCollegeID == null)
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                }
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                if (!pharmdcids.Contains(userCollegeID))
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                }
                int presentAY =
                   db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                       .Select(a => a.actualYear)
                       .FirstOrDefault();
                int academicyearId =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int enclosureId =
                    db.jntuh_enclosures.Where(e => e.documentName == "Pharm D BAS Faculty Letter")
                        .Select(e => e.id)
                        .FirstOrDefault();
                var PharmDletter =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == academicyearId)
                        .OrderByDescending(a => a.id)
                        .Select(e => e.path)
                        .FirstOrDefault();
                ViewBag.Pharmd = PharmDletter;
                return View();
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult PharmDuplaodfaculty(HttpPostedFileBase fileUploader, string collegeId)
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            if (Membership.GetUser() != null)
            {
                int[] pharmdcids = { 9, 24, 27, 30, 44, 47, 52, 55, 65, 90, 105, 110, 117, 127, 135, 139, 159, 169, 180, 202, 204, 206, 219, 234, 263, 267, 297, 319, 376, 379, 389, 392, 428, 442, 375 };
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0)
                {
                    userCollegeID = Convert.ToInt32(collegeId);
                }
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                if (!pharmdcids.Contains(userCollegeID))
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                }
                //To Save File in jntuh_college_enclosures
                string fileName = string.Empty;
                int presentAY =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int academicyearId =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int enclosureId =
                    db.jntuh_enclosures.Where(e => e.documentName == "Pharm D BAS Faculty Letter")
                        .Select(e => e.id)
                        .FirstOrDefault();
                var college_enclosures =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == academicyearId)
                        .Select(e => e)
                        .FirstOrDefault();
                jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                jntuh_college_enclosures.collegeID = userCollegeID;
                jntuh_college_enclosures.enclosureId = enclosureId;
                jntuh_college_enclosures.isActive = true;
                if (fileUploader != null)
                {
                    string ext = Path.GetExtension(fileUploader.FileName);
                    //DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1)
                    //if (!String.IsNullOrEmpty(college_enclosures.path))
                    //{
                    //    fileName = college_enclosures.path;
                    //}
                    //else
                    //{
                    fileName =
                        db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                        "_PharmD_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                    //}               
                    fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures"),
                        fileName));
                    jntuh_college_enclosures.path = fileName;
                }
                else if (!string.IsNullOrEmpty(college_enclosures.path))
                {
                    fileName = college_enclosures.path;
                    jntuh_college_enclosures.path = fileName;
                }

                if (college_enclosures == null)
                {
                    jntuh_college_enclosures.academicyearId = academicyearId;
                    jntuh_college_enclosures.createdBy = userID;
                    jntuh_college_enclosures.createdOn = DateTime.Now;
                    db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                    TempData["Success"] = "file saved successfully.";
                    db.SaveChanges();
                }
                else
                {
                    college_enclosures.path = fileName;
                    college_enclosures.updatedBy = userID;
                    college_enclosures.updatedOn = DateTime.Now;
                    db.Entry(college_enclosures).State = EntityState.Modified;
                    TempData["Success"] = "file updated successfully.";
                    db.SaveChanges();
                }
                return RedirectToAction("PharmDuplaodfaculty");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult Downladpdf()
        {
            return File("~/Content/BAS Faculty Circular.pdf", "application/pdf", "Pharm.D BAS Faculty Circular.pdf");
        }
    }
    public class collegeappealdeptfaculty
    {
        public string Departmentname { get; set; }
        public string DegreeName { get; set; }
        public string RegistrationNumber { get; set; }
        public string SpecializationName { get; set; }
    }
    public class MpharmacySpec
    {
        public int MPharmacyspecid { get; set; }
        public string MPharmacyspecname { get; set; }
    }
    public class CollegeFacultyWithIntakeReport
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int academicYearId { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }
        public int shiftId { get; set; }
        public string Shift { get; set; }
        public int specializationId { get; set; }
        public int DepartmentID { get; set; }
        public int DegreeId { get; set; }
        public int? degreeDisplayOrder { get; set; }
        public string collegeRandomCode { get; set; }
        public int ProposedIntake { get; set; }
        public int approvedIntake1 { get; set; }
        public int approvedIntake2 { get; set; }
        public int approvedIntake3 { get; set; }
        public int approvedIntake4 { get; set; }
        public int approvedIntake5 { get; set; }
        public bool ispercentage { get; set; }
        public bool isintakeeditable { get; set; }



        //Added this in 25-04-2017
        public int admittedIntake1 { get; set; }
        public int admittedIntake2 { get; set; }
        public int admittedIntake3 { get; set; }
        public int admittedIntake4 { get; set; }
        public int admittedIntake5 { get; set; }

        public int AICTESanctionIntake1 { get; set; }
        public int AICTESanctionIntake2 { get; set; }
        public int AICTESanctionIntake3 { get; set; }
        public int AICTESanctionIntake4 { get; set; }
        public int AICTESanctionIntake5 { get; set; }

        public int ExambranchIntake_R1 { get; set; }
        public int ExambranchIntake_R2 { get; set; }
        public int ExambranchIntake_R3 { get; set; }
        public int ExambranchIntake_R4 { get; set; }
        public int ExambranchIntake_R5 { get; set; }

        public int ExambranchIntake_L1 { get; set; }
        public int ExambranchIntake_L2 { get; set; }
        public int ExambranchIntake_L3 { get; set; }
        public int ExambranchIntake_L4 { get; set; }
        public int ExambranchIntake_L5 { get; set; }

        public int SanctionIntake1 { get; set; }
        public int SanctionIntake2 { get; set; }
        public int SanctionIntake3 { get; set; }
        public int SanctionIntake4 { get; set; }
        public int SanctionIntake5 { get; set; }

        public int totalAdmittedIntake { get; set; }
        //

        public bool AffiliationStatus2 { get; set; }
        public bool AffiliationStatus3 { get; set; }
        public bool AffiliationStatus4 { get; set; }

        public int division1 { get; set; }
        public int division2 { get; set; }
        public int division3 { get; set; }




        public int totalIntake { get; set; }
        public decimal requiredFaculty { get; set; }
        public int phdFaculty { get; set; }
        public int SpecializationsphdFaculty { get; set; }
        public int SpecializationspgFaculty { get; set; }
        public int pgFaculty { get; set; }
        public int ugFaculty { get; set; }
        public int totalFaculty { get; set; }
        public int specializationWiseFaculty { get; set; }
        public int PharmacyspecializationWiseFaculty { get; set; }
        public int facultyWithoutPANAndAadhaar { get; set; }
        public int newlyAddedFaculty { get; set; }

        public bool isActive { get; set; }
        public DateTime? nbaFrom { get; set; }
        public DateTime? nbaTo { get; set; }

        public bool? deficiency { get; set; }
        public bool? PHDdeficiency { get; set; }
        public bool? PHDBtechdeficiency { get; set; }
        public int shortage { get; set; }
        public IList<Lab> LabsListDefs { get; set; }
        public List<AnonymousLabclass> LabsListDefs1 { get; set; }
        public List<AnonymousMBAMACclass> MBAMACDetails { get; set; }
        public bool deficiencystatus { get; set; }
        public string RegistrationNumber { get; set; }
        //=====18-06-2015=====//
        public int FalseNameFaculty { get; set; }
        public int FalsePhotoFaculty { get; set; }
        public int FalsePANNumberFaculty { get; set; }
        public int FalseAadhaarNumberFaculty { get; set; }
        public int CertificatesIncompleteFaculty { get; set; }
        public int AbsentFaculty { get; set; }
        public int AvailableFaculty { get; set; }
        public int AvailablePHDFaculty { get; set; }

        public string PharmacyGroup1 { get; set; }


        public string PharmacySubGroup1 { get; set; }
        public int BPharmacySubGroup1Count { get; set; }
        public int BPharmacySubGroupRequired { get; set; }
        public string BPharmacySubGroupMet { get; set; }


        public string PharmacySpec1 { get; set; }
        public string PharmacySpec2 { get; set; }

        public IList<PharmacySpecilaizationList> PharmacySpecilaizationList { get; set; }

        //For collegeintake

        public List<CollegeIntakeExisting> CollegeIntakeExistings { get; set; }

        public string AffliationStatus { get; set; }
        public decimal BphramacyrequiredFaculty { get; set; }
        public decimal pharmadrequiredfaculty { get; set; }
        public decimal pharmadPBrequiredfaculty { get; set; }
        public int totalcollegefaculty { get; set; }
        public int SortId { get; set; }

        public IList<CollegeFacultyWithIntakeReport> FacultyWithIntakeReports { get; set; }
        public int BtechAdjustedFaculty { get; set; }
        public int specializationWiseFacultyPHDFaculty { get; set; }
        public IList<PhysicalLabMaster> PhysicalLabs { get; set; }

        public int Testrequriedfaculty { get; set; }
        public int Testavilablefaculty { get; set; }
    }
    public class PharmacySpecilaizationList
    {
        public string PharmacyspecName { get; set; }
        public string Specialization { get; set; }
        public int PharmacyspecWiseCount { get; set; }
    }
    public class AnonymousLabclass
    {
        public int? id { get; set; }
        public int? EquipmentID { get; set; }
        public string LabCode { get; set; }
        public string LabName { get; set; }
        public string EquipmentName { get; set; }
        public int year { get; set; }
        public int? Semester { get; set; }
        public int specializationId { get; set; }
        public string Department { get; set; }
    }
    public class AnonymousMBAMACclass
    {
        public int? id { get; set; }
        public string CollegeCode { get; set; }
        public int CollegeId { get; set; }
        public HttpPostedFileBase MACSupportingDoc { get; set; }
        public int? ComputerDeficiencyCount { get; set; }
    }
}
