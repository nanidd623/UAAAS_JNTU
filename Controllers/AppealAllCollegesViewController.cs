using System;
using System.Collections.Generic;
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
    public class AppealAllCollegesViewController : BaseController
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

        #region AllClasses & Methods
        public class AnonymousMBAMACclass
        {
            public int? id { get; set; }
            public string CollegeCode { get; set; }
            public int CollegeId { get; set; }
            public HttpPostedFileBase MACSupportingDoc { get; set; }
            public int? ComputerDeficiencyCount { get; set; }
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
            public string Specialization { get; set; }
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
            public int? degreeDisplayOrder { get; set; }
            public string collegeRandomCode { get; set; }
            public int approvedIntake1 { get; set; }
            public int approvedIntake2 { get; set; }
            public int approvedIntake3 { get; set; }
            public int approvedIntake4 { get; set; }
            public int approvedIntake5 { get; set; }
            public int totalIntake { get; set; }
            public decimal requiredFaculty { get; set; }
            public int phdFaculty { get; set; }
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
        }


        public class PharmacySpecilaizationList
        {
            public string PharmacyspecName { get; set; }
            public string Specialization { get; set; }
            public int PharmacyspecWiseCount { get; set; }
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            //approved
            if (flag == 1 && academicYearId != 8)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

            }
            else if (flag == 1 && academicYearId == 8)
            {
                var inta = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).FirstOrDefault();
                if (inta != null)
                {
                    intake = (int)inta.proposedIntake;
                }

            }
            else //admitted
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            }

            return intake;
        }
        #endregion
        

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var completedSubmissionColleges = new List<CompletedSubmission>();

            var IsEditableColleges = db.jntuh_appeal_college_edit_status.Select(editStatus => editStatus.collegeId).ToArray();
            var jntuh_colleges = db.jntuh_college.ToList();
            var jntuh_college_edit_statuss = db.jntuh_appeal_college_edit_status.ToList();
            foreach (var collegeId in IsEditableColleges)
            {
                //if (collegeId == 375) continue;
                var completedSubmissionCollege = new CompletedSubmission
                {
                    id = collegeId,
                    collegeId = collegeId,
                    collegeCode = jntuh_colleges.Where(editableCollege => editableCollege.id == collegeId)
                        .Select(editableCollege => editableCollege.collegeCode)
                        .FirstOrDefault(),
                    collegeName = jntuh_colleges.Where(editableCollege => editableCollege.id == collegeId)
                        .Select(editableCollege => editableCollege.collegeName)
                        .FirstOrDefault(),
                    submittedDate = jntuh_college_edit_statuss.Where(submitDate => submitDate.collegeId == collegeId)
                        .Select(submitDate => submitDate.updatedOn)
                        .FirstOrDefault()
                };
                completedSubmissionCollege.submitdate = completedSubmissionCollege.submittedDate != null ? Utilities.MMDDYY2DDMMYY(completedSubmissionCollege.submittedDate.ToString()) : string.Empty;
                completedSubmissionColleges.Add(completedSubmissionCollege);
            }
            ViewBag.Colleges = completedSubmissionColleges;
            ViewBag.Count = completedSubmissionColleges.Count();
            return View();
        }


        #region For PrincipalAppealAllviews

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult CollegeFacultyWithIntakePrincipal(string collegeID)
        {
            int collegeId = 0;
            if (!string.IsNullOrEmpty(collegeID))
            {
                collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            ViewBag.CollegeId = collegeId;
            var jntuhcollege = db.jntuh_college.AsNoTracking().ToList();
            var intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            var intakelist = new CollegeFacultyWithIntakeReport();
            #region Written By Srinivas
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            //if (CollegeDetails != null)
            //{
            //    EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
            //    EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
            //    collegeid = Convert.ToInt32(CollegeDetails.collegeId);
            //    PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

            //    if (currentDate >= EditFromDate && currentDate <= EditTODate)
            //    {
            //        if (PageEdible == false)
            //        {
            //            return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //        }
            //    }
            //    else
            //    {
            //        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //    }
            //}
            //else
            //{
            //    return RedirectToAction("College", "Dashboard");
            //}

            // Principal Details
            var strPrincipalRegno = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf.RegistrationNumber).FirstOrDefault();
            var principal = string.Empty;
            var Reason = string.Empty;
            var prinicpalexists = false;
            //Reg nos related online facultyIds
            var regdata = db.jntuh_registered_faculty.Where(rf => strPrincipalRegno == rf.RegistrationNumber).FirstOrDefault();
            ViewBag.principaldata = regdata;
            if (regdata != null)
            {
                //facultydata.LastName = regdata.LastName;
                //facultydata.RegistrationNumber = regdata.RegistrationNumber;
                if (regdata.Absent == true)
                {
                    Reason = "NOT AVAILABLE" + ",";
                }
                if (regdata.NotQualifiedAsperAICTE == true)
                {
                    Reason += "NOT QUALIFIED " + ",";
                }
                if (regdata.InvalidPANNumber == true)
                {
                    Reason += "NO PAN" + ",";
                }
                
                if (regdata.FalsePAN == true)
                {
                    Reason += "FALSE PAN" + ",";
                }
                if (regdata.NoSCM == true)
                {
                    Reason += "NO SCM/RATIFICATION" + ",";
                }
                if (regdata.IncompleteCertificates == true)
                {
                    Reason += "Incomplete Certificates" + ",";
                }
                if (regdata.PHDundertakingnotsubmitted == true)
                {
                    Reason += "No Undertaking" + ",";
                }
                if (regdata.Blacklistfaculy == true)
                {
                    Reason += "Blacklisted" + ",";
                }
                if (Reason != "")
                {
                    Reason = Reason.Substring(0, Reason.Length - 1);
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

            var jntuhAppealPrincipal = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
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
            return View(intakedetailsList);
        }


        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult PrincipalFacultyRegistrationNumber(string collegeId, string fid, int deficencycount, int departmentid, string degree, int specializationid)
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
        public ActionResult PrincipalFacultyRegistrationNumber(CollegeFaculty faculty)
        {
            TempData["Error"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var jntuh_appeal_faculty = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
            var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
            string[] RegistrationNumber = jntuh_appeal_faculty.Select(i => i.RegistrationNumber).ToArray();


            if (isRegisteredFaculty == null)
            {
                TempData["Error"] = "Invalid Principal Registration Number.";
                return RedirectToAction("CollegeFacultyWithIntakePrincipal", "AppealAllCollegesView", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (isExistingFaculty != null)
            {
                if (userCollegeID != isExistingFaculty.collegeId)
                {
                    TempData["Error"] = "Principal is already working in other JNTUH affiliated college.";
                    return RedirectToAction("CollegeFacultyWithIntakePrincipal", "AppealAllCollegesView", new { collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
            }

            var notificationPath = "~/Content/Upload/OnlineAppealDocuments/Principal/NotificationsReports";
            var selectioncommitteePath = "~/Content/Upload/OnlineAppealDocuments/Principal/SelectionCommitteeReports";
            var appointmentorderPath = "~/Content/Upload/OnlineAppealDocuments/Principal/AppointmentOrders";
            var joiningreportpath = "~/Content/Upload/OnlineAppealDocuments/Principal/JoiningReports";
            var phdundertakingdocpath = "~/Content/Upload/OnlineAppealDocuments/Principal/PhdUndertakingReports";
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
                UpdatedFaculty.RegistrationNumber = faculty.FacultyRegistrationNumber;
                UpdatedFaculty.existingFacultyId = isRegisteredFaculty.id;
                UpdatedFaculty.IdentifiedFor = faculty.facultyRecruitedFor;
                //UpdatedFaculty.DepartmentId = faculty.facultyDepartmentId;
                //UpdatedFaculty.SpecializationId = faculty.SpecializationId;

                //var jntuhDepartment = jntuh_deparment.Where(i => i.id == faculty.facultyDepartmentId).FirstOrDefault();
                //if (jntuhDepartment != null)
                //{
                //    UpdatedFaculty.DegreeId = jntuhDepartment.degreeId;
                //}
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


            return RedirectToAction("CollegeFacultyWithIntakePrincipal", "AppealAllCollegesView");
        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult AppealReverificationforPrincipal(string collegeId, string registrationnumber)
        {
            List<FacultyRegistration> facultyDetails = new List<FacultyRegistration>();

            if (!string.IsNullOrEmpty(collegeId))
            {
                int CollegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                //Reg nos related online facultyIds
                var rg = db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == registrationnumber && rf.collegeId == CollegeId).FirstOrDefault();

                if (rg != null)
                {
                    FacultyRegistration FR = new FacultyRegistration();
                    FR.RegistrationNumber = rg.RegistrationNumber;
                    FR.id = rg.id;
                    FR.CollegeId = rg.collegeId;
                    FR.DepartmentId = rg.DepartmentId;
                    FR.department = db.jntuh_department.Where(i => i.id == rg.DepartmentId).Select(i => i.departmentName).FirstOrDefault();
                    FR.FirstName = rg.FirstName + rg.MiddleName + rg.LastName;
                    FR.facultyPhoto = rg.Photo;
                    FR.DeactivationReason = rg.DeactivationReason;
                    FR.Absent = rg.Absent != null && (bool)rg.Absent;
                    FR.NOTQualifiedAsPerAICTE = rg.NotQualifiedAsperAICTE != null && (bool)rg.NotQualifiedAsperAICTE;
                    FR.NoSCM = rg.NoSCM != null && (bool)rg.NoSCM;
                    FR.PANNumber = rg.PANNumber;
                    FR.PHDundertakingnotsubmitted = rg.PHDundertakingnotsubmitted != null && (bool)rg.PHDundertakingnotsubmitted;
                    FR.BlacklistFaculty = rg.Blacklistfaculy != null && (bool)rg.Blacklistfaculy;
                    FR.FalsePAN = rg.FalsePAN != null && (bool)rg.FalsePAN;
                    FR.jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(i => i.facultyId == rg.id).ToList();
                    if (rg.InvalidPANNumber != null) FR.InvalidPANNo = (bool) rg.InvalidPANNumber;
                    facultyDetails.Add(FR);
                    ViewBag.collegeid = rg.collegeId;
                    ViewBag.Rgno = rg.RegistrationNumber;
                }

               
                //ViewBag.degree = db.jntuh_degree.Where(i => i.id == rg.jntuh_department.degreeId).Select(i => i.id).FirstOrDefault();
                //ViewBag.departmentid = rg.DepartmentId;
                //    ViewBag.specializationid=
            }
            return View(facultyDetails);

        }


        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult PrincipalAppealReverification(string collegeId, string fid, int deficencycount, int departmentid, string degree, string specializationid, string registrationnumber)
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
            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult PrincipalAppealFacultyRegistrationNumber(CollegeFaculty faculty)
        {
            TempData["Error"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            jntuh_registered_faculty isRegisteredFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            jntuh_college_faculty_registered isExistingFaculty = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(r => r).FirstOrDefault();
            var jntuh_appeal_faculty = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
            var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
            string[] RegistrationNumber = jntuh_appeal_faculty.Select(i => i.RegistrationNumber).ToArray();

            var physicalpresencpath = "~/Content/Upload/OnlineAppealDocuments/Principal/PhysicalPresenceReports";
            var phdundertakingdocpath = "~/Content/Upload/OnlineAppealDocuments/Principal/PhdUndertakingReports";
            if (!RegistrationNumber.Contains(faculty.FacultyRegistrationNumber.Trim()))
            {
                //int FacultyId = db.jntuh_college_faculty_registered.Where(F => F.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(F => F.id).FirstOrDefault();
                //jntuh_college_faculty_registered fToDelete = db.jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(i => i).FirstOrDefault();

                jntuh_appeal_principal_registered UpdatedFaculty = new jntuh_appeal_principal_registered();
                UpdatedFaculty.collegeId = isRegisteredFaculty.collegeId != null
                    ? (int)isRegisteredFaculty.collegeId
                    : 0;
                UpdatedFaculty.collegeId = faculty.collegeId;
                UpdatedFaculty.RegistrationNumber = faculty.FacultyRegistrationNumber;
                UpdatedFaculty.existingFacultyId = isRegisteredFaculty.id;

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
                        i => i.RegistrationNumber == faculty.FacultyRegistrationNumber).FirstOrDefault();


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

            return RedirectToAction("AppealReverificationforPrincipal", "AppealAllCollegesView", new { @collegeId = UAAAS.Models.Utilities.EncryptString(faculty.collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]), @registrationnumber = faculty.FacultyRegistrationNumber });
        }

        #endregion

        #region For AllFacultyView

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult ALLCollegeFacultyWithIntakeFaculty(string collegeID)
        {
            int collegeId = 0;
            if (!string.IsNullOrEmpty(collegeID))
            {
                collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            var integreatedIds = new[] { 9, 18, 39, 42, 75, 140, 180, 332, 364, 375 };

            if (integreatedIds.Contains(collegeId))
            {
                return RedirectToAction("IntegratedCollegesFacultyWithIntake", "AppealAllCollegesView" ,new{collegeID = collegeId});
            }

            var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370};

            if (pharmacyids.Contains(collegeId))
            {
                return RedirectToAction("CollegeFacultyWithIntakeFaculty", "AppealAllCollegesView",new {collegeID = collegeId});
            }

            return RedirectToAction("CollegeFacultyBtechWithIntake", "AppealAllCollegesView", new { collegeID = collegeId });
        }


        #region For IntegratedFaculty


        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult IntegratedCollegesFacultyWithIntake(int collegeID)
        {
            ViewBag.collegeID = collegeID;
            return View();
        }


        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult IntegratedBtechFacultyWithIntake(int collegeID)
        {
            #region new code
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int collegeId = collegeID;
            ViewBag.CollegeId = collegeID;
            #region Written By Srinivas
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            //if (CollegeDetails != null)
            //{
            //    EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
            //    EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
            //    collegeid = Convert.ToInt32(CollegeDetails.collegeId);
            //    PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

            //    if (currentDate >= EditFromDate && currentDate <= EditTODate)
            //    {
            //        if (PageEdible == false)
            //        {
            //            return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //        }
            //    }
            //    else
            //    {
            //        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //    }
            //}
            //else
            //{
            //    return RedirectToAction("College", "Dashboard");
            //}

            #endregion

            #region Faculty
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();

            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            var firstOrDefault = colleges.FirstOrDefault(i => i.collegeId == collegeID);
            if (firstOrDefault != null)
                ViewBag.CollegeName = firstOrDefault.collegeName;
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

                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                //Reg nos related online facultyIds
                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& (rf.collegeId == null || rf.collegeId == collegeId)
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var regfacultywithoutdepts = registeredFaculty.Where(r => r.DepartmentId == null).Select(i => i.type);

                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.PHDundertakingnotsubmitted != true && rf.Blacklistfaculy != true
                                                        && (rf.type != "Adjunct"))).Select(rf => new
                                                        {
                                                            RegistrationNumber = rf.RegistrationNumber,
                                                            Department = rf.jntuh_department.departmentName,
                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                            IsApproved = rf.isApproved,
                                                            PanNumber = rf.PANNumber,
                                                            AadhaarNumber = rf.AadhaarNumber,
                                                            TotalExperience = rf.TotalExperience
                                                        }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    TotalExperience = rf.TotalExperience
                }).Where(e => e.Department != null).ToList();

                var jjj = jntuh_registered_faculty.Where(i => i.Department == "CSE").ToList();
                var pharmacydeptids = new[] { 26, 27, 36, 39 };
                collegeIntakeExisting = collegeIntakeExisting.Where(i => !pharmacydeptids.Contains(i.DepartmentID)).ToList();
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
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                                                    (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3);
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
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" &&
                                f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department &&
                                        f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
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


                    intakedetailsList.Add(intakedetails);
                }
                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
                int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
                var jntuh_departments = db.jntuh_department.ToList();
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
                intakedetailsList = intakedetailsList.Where(i => i.shiftId == 1).ToList();
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

                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = false;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 0;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = false);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                        else if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
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
                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = false;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 0;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = false);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                        else if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = true;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 2;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = true);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }
                    //if (adjustedPHDFaculty > 0)
                    //{
                    //    item.PHDdeficiency = false;
                    //}
                    //else if (item.approvedIntake1 > 0)
                    //{
                    //    item.PHDdeficiency = true;
                    //    item.AvailablePHDFaculty = 1;
                    //}
                    if (strOtherDepartments.Contains(item.Department))
                    {
                        item.totalIntake = totalBtechFirstYearIntake;
                        item.requiredFaculty = Math.Ceiling((decimal)firstYearRequired);
                    }
                    deptloop++;
                }
            #endregion


                #region Faculty Appeal Deficiency Status
                var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
                var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
                foreach (var item in intakedetailsList.Where(i => i.shiftId == 1).ToList())
                {

                    var deparment = jntuh_deparment.FirstOrDefault(i => i.id == item.DepartmentID);
                    if (deparment != null)
                    {
                        var jntuh_departmentcount =
                        jntuh_appeal_faculty.Where(
                            i =>
                                i.DepartmentId == item.DepartmentID && i.SpecializationId == item.specializationId &&
                                i.DegreeId == deparment.degreeId && i.collegeId == collegeId && i.NOtificationReport != null).ToList();
                        var facultydefcount = (int)Math.Ceiling(item.requiredFaculty) - item.BtechAdjustedFaculty;
                        if (item.PHDdeficiency == true)
                        {
                            facultydefcount = facultydefcount + item.AvailablePHDFaculty;
                        }
                        if (facultydefcount <= jntuh_departmentcount.Count && jntuh_departmentcount.Count != 0)
                        {
                            item.deficiencystatus = true;
                        }
                    }
                }


                #endregion

            }
            return View(intakedetailsList.Where(i => i.shiftId == 1).ToList());
            #endregion
        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult IntegratedPharmacyFacultyWithIntake(int collegeID)
        {
            #region new code
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int collegeId = collegeID;
            ViewBag.CollegeId = collegeID;
            #region Written By Srinivas
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            //if (CollegeDetails != null)
            //{
            //    EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
            //    EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
            //    collegeid = Convert.ToInt32(CollegeDetails.collegeId);
            //    PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

            //    if (currentDate >= EditFromDate && currentDate <= EditTODate)
            //    {
            //        if (PageEdible == false)
            //        {
            //            return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //        }
            //    }
            //    else
            //    {
            //        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //    }
            //}
            //else
            //{
            //    return RedirectToAction("College", "Dashboard");
            //}

            // Principal Details
            string strPrincipalRegno = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf.RegistrationNumber).FirstOrDefault();

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
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

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

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();
                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
                //Reg nos related online facultyIds
                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                   : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& (rf.collegeId == null || rf.collegeId == collegeId)
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var regfacultywithoutdepts = registeredFaculty.Where(r => r.DepartmentId == null).Select(i => i.type);

                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && (rf.PHDundertakingnotsubmitted != true)
                                                        && (rf.Notin116 != true) && (rf.Blacklistfaculy != true) && rf.DepartmentId == 61)).Select(rf => new
                                                        {
                                                            RegistrationNumber = rf.RegistrationNumber,
                                                            Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                            IsApproved = rf.isApproved,
                                                            PanNumber = rf.PANNumber,
                                                            AadhaarNumber = rf.AadhaarNumber,
                                                            TotalExperience = rf.TotalExperience,
                                                            jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education
                                                        }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    TotalExperience = rf.TotalExperience,
                    registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
                }).ToList();
                ViewBag.ALLTotalCollegeFaculty = jntuh_registered_faculty.Count;
                var Bpharmacyintake = 0;
                decimal BpharmacyrequiredFaculty = 0;
                var pharmacydeptids = new[] { 26, 27, 36, 39 };
                collegeIntakeExisting = collegeIntakeExisting.Where(i => pharmacydeptids.Contains(i.DepartmentID)).ToList();
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
                        var total = intakedetails.totalIntake > 400 ? 100 : 60;
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



                List<CollegeFacultyWithIntakeReport> facultyCounts = intakedetailsList.ToList();
                int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
                var degrees = db.jntuh_degree.ToList();
                var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 150);
                int remainingFaculty = 0;
                int remainingPHDFaculty = 0;
                decimal departmentWiseRequiredFaculty = 0;
                var distDeptcount = 1;
                var deptloop = 1;
                foreach (var item in facultyCounts)
                {
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

                    var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                    if (deptloop == 1)
                    {
                        if (rFaculty <= tFaculty)
                        {
                            minimumRequirementMet = "NO";
                            item.deficiency = false;
                            remainingFaculty = tFaculty - rFaculty;
                            adjustedFaculty = rFaculty;//tFaculty
                            item.totalFaculty = adjustedFaculty;
                        }
                        else
                        {
                            minimumRequirementMet = "YES";
                            item.deficiency = true;
                            adjustedFaculty = tFaculty;
                            facultyShortage = rFaculty - tFaculty;
                            item.totalFaculty = adjustedFaculty;
                        }

                        remainingPHDFaculty = item.phdFaculty;

                        if (remainingPHDFaculty > 0 && (degreeType.Equals("PG") || degreeType.Equals("UG"))) //degreeType.Equals("PG")
                        {
                            //adjustedPHDFaculty = 1;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }
                    else
                    {
                        if (rFaculty <= tFaculty)
                        {
                            minimumRequirementMet = "NO";
                            item.deficiency = false;
                            remainingFaculty = remainingFaculty - rFaculty;
                            adjustedFaculty = rFaculty;
                            item.totalFaculty = adjustedFaculty;
                        }
                        else
                        {
                            minimumRequirementMet = "YES";
                            item.deficiency = true;
                            adjustedFaculty = remainingFaculty;
                            item.totalFaculty = adjustedFaculty;
                            facultyShortage = rFaculty - remainingFaculty;
                            remainingFaculty = 0;
                        }
                        remainingPHDFaculty = item.phdFaculty;
                        if (remainingPHDFaculty > 0 && (degreeType.Equals("PG") || degreeType.Equals("UG")))
                        {
                            //adjustedPHDFaculty = 1;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }
                    if (adjustedPHDFaculty > 0)
                    {
                        item.PHDdeficiency = false;
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
                    else if (item.approvedIntake1 > 0)
                    {
                        item.PHDdeficiency = true;
                        item.AvailablePHDFaculty = 1;
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
                        var total = intakedetails.totalIntake > 400 ? 100 : 60;
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
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()));
                                    break;
                                case 121://Pharmacognosy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacognosy/Pharma Biotechnology/Pharmacology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacognosy".ToUpper()));
                                    break;
                                case 117://Pharmaceutical Chemistry
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/Pharmaceuticalchemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()));
                                    break;
                                case 119://Pharmaceutical Technology (2011-12)
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization.Contains("Bio-Technology".ToUpper()) || f.registered_faculty_specialization.Contains("Pharmaceutical Technology (2011-12)".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()));
                                    break;
                                case 123://Quality Assurance
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
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
                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practise".ToUpper()) +
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
                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practise".ToUpper()) +
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
                ViewBag.PharmDrequiredFaculty = PharmDrequiredFaculty;
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
                intakedetailsList = intakedetailsList.Where(i => i.shiftId == 1).ToList();
                foreach (var item in intakedetailsList)
                {
                    var deparment = jntuh_deparment.FirstOrDefault(i => i.id == item.DepartmentID);
                    if (deparment != null)
                    {
                        var jntuh_departmentcount =
                        jntuh_appeal_faculty.Where(
                            i =>
                                i.DepartmentId == item.DepartmentID && i.SpecializationId == item.specializationId &&
                                i.DegreeId == deparment.degreeId && i.collegeId == collegeId && i.NOtificationReport != null).ToList();
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
                                facultydefcount = Allgroupscount;//(int)lessfaculty + 
                            }
                            else if (Allgroupscount > lessfaculty)
                            {
                                facultydefcount = Allgroupscount;//+ (int)lessfaculty
                            }
                        }

                        if (item.Department == "B.Pharmacy")
                        {
                            if (Allgroupscount > 0) { item.deficiency = true; }
                            ViewBag.BpharmacyRequired = facultydefcount;
                        }

                        if (item.PharmacyspecializationWiseFaculty < 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
                        {
                            facultydefcount = (int)Math.Ceiling(item.requiredFaculty) + 1;
                        }
                        if (item.PharmacyspecializationWiseFaculty >= 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
                        {
                            facultydefcount = (int)Math.Ceiling(item.requiredFaculty);
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

            }
            return View(intakedetailsList);
            #endregion
        }


        #endregion
        

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult CollegeFacultyBtechWithIntake(int collegeID)
        {
            #region new code
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int collegeId = collegeID;
            ViewBag.CollegeId = collegeID;
            #region Written By Srinivas
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            //if (CollegeDetails != null)
            //{
            //    EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
            //    EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
            //    collegeid = Convert.ToInt32(CollegeDetails.collegeId);
            //    PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

            //    if (currentDate >= EditFromDate && currentDate <= EditTODate)
            //    {
            //        if (PageEdible == false)
            //        {
            //            return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //        }
            //    }
            //    else
            //    {
            //        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //    }
            //}
            //else
            //{
            //    return RedirectToAction("College", "Dashboard");
            //}

            // Principal Details
            string strPrincipalRegno = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf.RegistrationNumber).FirstOrDefault();

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

            #region Faculty
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();

            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            var firstOrDefault = colleges.FirstOrDefault(i => i.collegeId == collegeID);
            if (firstOrDefault != null)
                ViewBag.CollegeName = firstOrDefault.collegeName;
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

                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                //Reg nos related online facultyIds
                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& (rf.collegeId == null || rf.collegeId == collegeId)
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var regfacultywithoutdepts = registeredFaculty.Where(r => r.DepartmentId == null).Select(i => i.type);

                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.PHDundertakingnotsubmitted != true && rf.Blacklistfaculy != true
                                                        && (rf.type != "Adjunct"))).Select(rf => new
                                                        {
                                                            RegistrationNumber = rf.RegistrationNumber,
                                                            Department = rf.jntuh_department.departmentName,
                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                            IsApproved = rf.isApproved,
                                                            PanNumber = rf.PANNumber,
                                                            AadhaarNumber = rf.AadhaarNumber,
                                                            TotalExperience = rf.TotalExperience
                                                        }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    TotalExperience = rf.TotalExperience
                }).Where(e => e.Department != null).ToList();

                var jjj = jntuh_registered_faculty.Where(i => i.Department == "CSE").ToList();
                var pharmacydeptids = new[] { 26, 27, 36, 39 };
                collegeIntakeExisting = collegeIntakeExisting.Where(i => !pharmacydeptids.Contains(i.DepartmentID)).ToList();
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
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                                                    (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3);
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
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" &&
                                f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department &&
                                        f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
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


                    intakedetailsList.Add(intakedetails);
                }
                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
                int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
                var jntuh_departments = db.jntuh_department.ToList();
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
                intakedetailsList = intakedetailsList.Where(i => i.shiftId == 1).ToList();
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

                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = false;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 0;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = false);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                        else if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
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
                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = false;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 0;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = false);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                        else if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")) && item.requiredFaculty > 0)
                        {
                            //adjustedPHDFaculty = 1;
                            item.PHDdeficiency = true;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            item.AvailablePHDFaculty = 2;
                            intakedetailsList.Where(i => i.Department == item.Department && i.Degree == "B.Tech").ToList().ForEach(c => c.PHDBtechdeficiency = true);
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }
                    //if (adjustedPHDFaculty > 0)
                    //{
                    //    item.PHDdeficiency = false;
                    //}
                    //else if (item.approvedIntake1 > 0)
                    //{
                    //    item.PHDdeficiency = true;
                    //    item.AvailablePHDFaculty = 1;
                    //}
                    if (strOtherDepartments.Contains(item.Department))
                    {
                        item.totalIntake = totalBtechFirstYearIntake;
                        item.requiredFaculty = Math.Ceiling((decimal)firstYearRequired);
                    }
                    deptloop++;
                }
            #endregion


                #region Faculty Appeal Deficiency Status
                var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
                var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
                foreach (var item in intakedetailsList.Where(i => i.shiftId == 1).ToList())
                {

                    var deparment = jntuh_deparment.FirstOrDefault(i => i.id == item.DepartmentID);
                    if (deparment != null)
                    {
                        var jntuh_departmentcount =
                        jntuh_appeal_faculty.Where(
                            i =>
                                i.DepartmentId == item.DepartmentID && i.SpecializationId == item.specializationId &&
                                i.DegreeId == deparment.degreeId && i.collegeId == collegeId && i.NOtificationReport != null).ToList();
                        var facultydefcount = (int)Math.Ceiling(item.requiredFaculty) - item.BtechAdjustedFaculty;
                        if (item.PHDdeficiency == true)
                        {
                            facultydefcount = facultydefcount + item.AvailablePHDFaculty;
                        }
                        if (facultydefcount <= jntuh_departmentcount.Count && jntuh_departmentcount.Count != 0)
                        {
                            item.deficiencystatus = true;
                        }
                    }
                }


                #endregion


                #region Principal Appeal Deficiency Status

                var jntuhAppealPrincipal = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
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
            return View(intakedetailsList.Where(i => i.shiftId == 1).ToList());
            #endregion
        }


        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult CollegeFacultyWithIntakeFaculty(int collegeID)
        {

            #region new code
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int collegeId = collegeID;
            ViewBag.CollegeId = collegeID;
            #region Written By Srinivas
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            //if (CollegeDetails != null)
            //{
            //    EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
            //    EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
            //    collegeid = Convert.ToInt32(CollegeDetails.collegeId);
            //    PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

            //    if (currentDate >= EditFromDate && currentDate <= EditTODate)
            //    {
            //        if (PageEdible == false)
            //        {
            //            return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //        }
            //    }
            //    else
            //    {
            //        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //    }
            //}
            //else
            //{
            //    return RedirectToAction("College", "Dashboard");
            //}

            // Principal Details
            string strPrincipalRegno = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf.RegistrationNumber).FirstOrDefault();

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

            #region Faculty
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();

            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            var firstOrDefault = colleges.FirstOrDefault(i => i.collegeId == collegeID);
            if (firstOrDefault != null)
                ViewBag.CollegeName = firstOrDefault.collegeName;
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

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();
                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
                //Reg nos related online facultyIds
                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                   : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& (rf.collegeId == null || rf.collegeId == collegeId)
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var regfacultywithoutdepts = registeredFaculty.Where(r => r.DepartmentId == null).Select(i => i.type);

                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && (rf.PHDundertakingnotsubmitted != true)
                                                        && (rf.Notin116 != true) && (rf.Blacklistfaculy != true))).Select(rf => new
                                                        {
                                                            RegistrationNumber = rf.RegistrationNumber,
                                                            Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                            IsApproved = rf.isApproved,
                                                            PanNumber = rf.PANNumber,
                                                            AadhaarNumber = rf.AadhaarNumber,
                                                            TotalExperience = rf.TotalExperience,
                                                            jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education
                                                        }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    TotalExperience = rf.TotalExperience,
                    registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
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
                        var total = intakedetails.totalIntake > 400 ? 100 : 60;
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



                List<CollegeFacultyWithIntakeReport> facultyCounts = intakedetailsList.ToList();
                int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
                var degrees = db.jntuh_degree.ToList();
                var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 150);
                int remainingFaculty = 0;
                int remainingPHDFaculty = 0;
                decimal departmentWiseRequiredFaculty = 0;
                var distDeptcount = 1;
                var deptloop = 1;
                foreach (var item in facultyCounts)
                {
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

                    var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                    if (deptloop == 1)
                    {
                        if (rFaculty <= tFaculty)
                        {
                            minimumRequirementMet = "NO";
                            item.deficiency = false;
                            remainingFaculty = tFaculty - rFaculty;
                            adjustedFaculty = rFaculty;//tFaculty
                            item.totalFaculty = adjustedFaculty;
                        }
                        else
                        {
                            minimumRequirementMet = "YES";
                            item.deficiency = true;
                            adjustedFaculty = tFaculty;
                            facultyShortage = rFaculty - tFaculty;
                            item.totalFaculty = adjustedFaculty;
                        }

                        remainingPHDFaculty = item.phdFaculty;

                        if (remainingPHDFaculty > 0 && (degreeType.Equals("PG") || degreeType.Equals("UG"))) //degreeType.Equals("PG")
                        {
                            //adjustedPHDFaculty = 1;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }
                    else
                    {
                        if (rFaculty <= tFaculty)
                        {
                            minimumRequirementMet = "NO";
                            item.deficiency = false;
                            remainingFaculty = remainingFaculty - rFaculty;
                            adjustedFaculty = rFaculty;
                            item.totalFaculty = adjustedFaculty;
                        }
                        else
                        {
                            minimumRequirementMet = "YES";
                            item.deficiency = true;
                            adjustedFaculty = remainingFaculty;
                            item.totalFaculty = adjustedFaculty;
                            facultyShortage = rFaculty - remainingFaculty;
                            remainingFaculty = 0;
                        }
                        remainingPHDFaculty = item.phdFaculty;
                        if (remainingPHDFaculty > 0 && (degreeType.Equals("PG") || degreeType.Equals("UG")))
                        {
                            //adjustedPHDFaculty = 1;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }
                    if (adjustedPHDFaculty > 0)
                    {
                        item.PHDdeficiency = false;
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
                    else if (item.approvedIntake1 > 0)
                    {
                        item.PHDdeficiency = true;
                        item.AvailablePHDFaculty = 1;
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
                        var total = intakedetails.totalIntake > 400 ? 100 : 60;
                        bpharmacyintake = total;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
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
                        if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)// && group5Subcount >= 2 && group6Subcount >= (bpharmacyintake >= 100 ? 3 : 2)
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
                    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                    //if (conditionbpharm == "No")
                    //{
                    //    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                    //}
                    //else
                    //{
                    //    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
                    //}


                }


                else if (PharmDrequiredFaculty == 0 && conditionbpharm == "No")
                {
                    ViewBag.PharmaDCondition = "No";
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
                    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                    //if (conditionbpharm == "No" && conditionpharmd == "No")
                    //{
                    //    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                    //}
                    //else
                    //{
                    //    intakedetailsList.FirstOrDefault().FacultyWithIntakeReports.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
                    //}

                }

                else if (PharmDPBrequiredFaculty == 0 && conditionbpharm == "No")
                {
                    ViewBag.PharmaDPBCondition = "No";
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
                                i.DegreeId == deparment.degreeId && i.collegeId == collegeId && i.NOtificationReport != null).ToList();
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
                                facultydefcount = Allgroupscount;//(int)lessfaculty + 
                            }
                            else if (Allgroupscount > lessfaculty)
                            {
                                facultydefcount = Allgroupscount;//+ (int)lessfaculty
                            }
                        }

                        if (item.Department == "B.Pharmacy")
                        {
                            if (Allgroupscount > 0) { item.deficiency = true; }
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

            }
            return View(intakedetailsList);
            #endregion

        }



        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult CollegeFacultyAddedDetails(int collegeId)
        {
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            List<jntuh_appeal_faculty_registered> facultydetails = new List<jntuh_appeal_faculty_registered>();
            if (collegeId != 0)
            {
                facultydetails = db.jntuh_appeal_faculty_registered.Where(i => i.collegeId == collegeId).Select(i => i).ToList();
            }
            return View(facultydetails);
        }

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult CollegePrincipalAddedDetails(int collegeId)
        {
            //var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //var collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var facultydetails = new List<jntuh_appeal_principal_registered>();
            if (collegeId != 0)
            {
                facultydetails = db.jntuh_appeal_principal_registered.Where(i => i.collegeId == collegeId).Select(i => i).ToList();
            }
            return View(facultydetails);
        }

        #endregion

        #region ForLabsView

        [Authorize(Roles = "Admin,College")]
        [HttpGet]
        public ActionResult LabsForAppeal(string collegeID)
        {
            var collegeId = 0;
            if (!string.IsNullOrEmpty(collegeID))
            {
                collegeId = Convert.ToInt32(Utilities.DecryptString(collegeID.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var facultyCounts = new List<CollegeFacultyWithIntakeReport>();
            var collegeLabAnonymousLabclass = new List<AnonymousLabclass>();
            var collegefaculty = new CollegeFacultyWithIntakeReport()
            {
                collegeId = collegeId
            };
            facultyCounts.Add(collegefaculty);
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();
            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            var firstDefault = colleges.FirstOrDefault(i => i.collegeId == collegeId);
            if (firstDefault != null)
                ViewBag.CollegeName = firstDefault.collegeName;
            #region CollegeEditStatus
            var CollegeDetails = db.jntuh_appeal_college_edit_status.Where(C => C.collegeId == collegeId).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();

            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            int collegeid = 0;
            bool PageEdible = false;
            //if (CollegeDetails != null)
            //{
            //    EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
            //    EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
            //    collegeid = Convert.ToInt32(CollegeDetails.collegeId);
            //    PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);

            //    if (currentDate >= EditFromDate && currentDate <= EditTODate)
            //    {
            //        if (PageEdible == false)
            //        {
            //            return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //        }
            //    }
            //    else
            //    {
            //        return RedirectToAction("ViewCollegeFacultyWithIntake", "FacultyVerification");
            //    }
            //}
            //else
            //{
            //    return RedirectToAction("College", "Dashboard");
            //}
            #endregion
            #region For labs
            var jntuh_specialization = db.jntuh_specialization.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();

            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e.specializationId).Distinct().ToArray();
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
            var jntuh_lab_masters = db.jntuh_lab_master.AsNoTracking().ToList();
            if (DegreeIDs.Contains(4) && CollegeAffiliationStatus == "Yes")
            {
                jntuh_lab_masters = jntuh_lab_masters.Where(l => l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)).ToList();
            }
            else
            {
                jntuh_lab_masters = jntuh_lab_masters.Where(l => specializationIds.Contains(l.SpecializationID)).ToList();
            }
            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabAnonymousLabclass = jntuh_lab_masters
                                                     .Select(l => new AnonymousLabclass
                                                     {
                                                         id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                                                         EquipmentID = l.id,
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

                collegeLabAnonymousLabclass = jntuh_lab_masters
                                                   .Where(l => l.Labcode != "TMP-CL")
                                                   .Select(l => new AnonymousLabclass
                                                   {
                                                       id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                                                       EquipmentID = l.id,
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

            var collegeEquipments = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeId).Select(l => l.EquipmentID).Distinct().ToArray();

            //var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { EquipmentID = c.id, LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
            //                           .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();
            //list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

            var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeId && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

            foreach (var coll in collegeLabAnonymousLabclass)
            {
                var deptname = jntuh_specialization.FirstOrDefault(i => i.id == coll.specializationId).jntuh_department.departmentName;
                coll.Department = deptname;
                coll.Specialization = jntuh_specialization.FirstOrDefault(i => i.id == coll.specializationId).specializationName;
            }


            //collegeLabAnonymousLabclass = collegeLabAnonymousLabclass.ToList().ForEach(i => i.DepartmentId = jntuh_specialization.FirstOrDefault(l => l.id == i.specializationId).departmentId);

            var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();


            list1 = list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

            #region this code written by suresh

            int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

            int[] clgequipmentIDs =
                db.jntuh_college_laboratories.Where(
                    l => l.CollegeID == collegeId && labequipmentIds.Contains(l.EquipmentID))
                    .Select(i => i.EquipmentID)
                    .ToArray();

            list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID))
                    .ToList();


            #endregion
            if (facultyCounts.Count > 0)
            {
                facultyCounts.FirstOrDefault().LabsListDefs1 = list1.ToList();
                //facultyCounts.FirstOrDefault().LabsListDefs = list1;
            }
            //facultyCounts.FirstOrDefault().LabsListDefs1 = list1.ToList();
            //ViewBag.labslist = list;
            //ViewBag.labslistcount = list.Count;
            //TempData["labslist"] = list;
            #endregion
            List<AnonymousMBAMACclass> MBAMACDetails = new List<AnonymousMBAMACclass>();
            if (collegeId != null)
            {
                //Commented on 18-06-2018 by Narayana Reddy
                //var mbadef = db.jntuh_appeal_mbadeficiency.FirstOrDefault(i => i.CollegeId == collegeId);
                //if (mbadef != null)
                //{
                    var firstOrDefault = db.jntuh_college.FirstOrDefault(i => i.id == collegeId);
                    if (firstOrDefault != null)
                    {
                        var colcode = firstOrDefault.collegeCode;
                        var macadreess = new AnonymousMBAMACclass();
                        macadreess.CollegeId = collegeId;
                        macadreess.CollegeCode = colcode;
                        //macadreess.ComputerDeficiencyCount = mbadef.ComputersDeficencyCount;
                        //macadreess.id = mbadef.Id;
                        MBAMACDetails.Add(macadreess);
                    }
                    var collegeFacultyWithIntakeReport = facultyCounts.FirstOrDefault();
                    if (collegeFacultyWithIntakeReport != null)
                        collegeFacultyWithIntakeReport.MBAMACDetails = MBAMACDetails;
                //}

                var collegeids = new[] { 343, 13, 101, 67, 394 };
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

        #endregion
    }
}
