using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using it = iTextSharp.text;
using System.Globalization;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.html;
using iTextSharp.text.xml;
using System.Web.Configuration;
using System.Data.OleDb;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using Utilities = UAAAS.Models.Utilities;

namespace UAAAS.Controllers.College
{
    public class AppealFormatController : BaseController
    {

        private uaaasDBContext db = new uaaasDBContext();
        private string serverURL;

        #region CustomClasses

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

            public List<CollegeIntakeExisting> CollegeIntakeSupportingDocuments { get; set; }

            public string AffliationStatus { get; set; }
            public decimal BphramacyrequiredFaculty { get; set; }
            public decimal pharmadrequiredfaculty { get; set; }
            public decimal pharmadPBrequiredfaculty { get; set; }
            public int totalcollegefaculty { get; set; }
            public int SortId { get; set; }

            public IList<CollegeFacultyWithIntakeReport> FacultyWithIntakeReports { get; set; }
            public int BtechAdjustedFaculty { get; set; }
            public int specializationWiseFacultyPHDFaculty { get; set; }
            public IList<CollegeFaculty> CollegeFaculties { get; set; }
            public string Remarks { get; set; }
            public string FurtherAppealSupportingDoc { get; set; }
            public string DeclarationPath { get; set; }
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
        }

        public class AnonymousMBAMACclass
        {
            public int? id { get; set; }
            public string CollegeCode { get; set; }
            public int CollegeId { get; set; }
            public HttpPostedFileBase MACSupportingDoc { get; set; }
            public int? ComputerDeficiencyCount { get; set; }
        }

        public class PharmacySpecilaizationList
        {
            public string PharmacyspecName { get; set; }
            public string Specialization { get; set; }
            public int PharmacyspecWiseCount { get; set; }
        }

        private int GetIntake(int? collegeId, int academicYearId, int specializationId, int shiftId, int flag)
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


        #region NewCodeRequirement

        //Appeal Faculty Details
        [Authorize(Roles = "Admin")]
        public ActionResult CollegeAppealDetails(int? collegeId)
        {

            var colgids = db.jntuh_appeal_college_edit_status.Where(i => i.IsCollegeEditable == false).GroupBy(e => e.collegeId).Select(e => e.Key).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive && colgids.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            var appealfacultyDetails = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
            var principalDetails = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
            var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_specialization = db.jntuh_specialization.AsNoTracking().ToList();
            var facultyDetails = new List<CollegeFacultyWithIntakeReport> { new CollegeFacultyWithIntakeReport() };
            var facultyWithIntakeReport = facultyDetails.FirstOrDefault();
            if (facultyWithIntakeReport != null)
            {
                facultyWithIntakeReport.CollegeFaculties = new List<CollegeFaculty>();
                facultyWithIntakeReport.LabsListDefs1 = new List<AnonymousLabclass>();
                facultyWithIntakeReport.CollegeIntakeExistings = new List<CollegeIntakeExisting>();
                facultyWithIntakeReport.CollegeIntakeSupportingDocuments = new List<CollegeIntakeExisting>();
            }
            #region Faculty
            if (collegeId != null)
            {
                ViewBag.collegeId = collegeId;
                appealfacultyDetails = appealfacultyDetails.Where(e => e.collegeId == collegeId).ToList();
                if (appealfacultyDetails.Count > 0)
                {
                    foreach (var item in appealfacultyDetails)
                    {
                        var data = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                        var Reason1 = string.Empty;
                        var faculty = new CollegeFaculty();
                        var labs = new AnonymousLabclass();
                        var intake = new CollegeIntakeExisting();
                        var intakesupportingDocuments = new CollegeIntakeExisting();
                        faculty.FacultyRegistrationNumber = item.RegistrationNumber;
                        faculty.collegeId = item.collegeId;
                        //  faculty.CollegeName = jntuh_college.Where(i => i.id == item.collegeId).Select(i => i.collegeName).FirstOrDefault();
                        faculty.id = data.id;
                        faculty.facultyFirstName = data.FirstName;
                        faculty.facultyLastName = data.LastName;
                        faculty.facultySurname = data.MiddleName;
                        faculty.ViewNotificationDocument = item.NOtificationReport;
                        faculty.ViewAppointmentOrderDocument = item.AppointMentOrder;
                        faculty.ViewJoiningReportDocument = item.JoiningOrder;
                        faculty.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                        faculty.ViewAppealReverificationSupportDoc = item.AppealReverificationSupportingDocument;
                        var jntuhDepartment = jntuh_department.FirstOrDefault(i => i.id == item.DepartmentId);
                        if (jntuhDepartment != null)
                            faculty.department = jntuhDepartment.departmentName;
                        var degreeid = data.jntuh_registered_faculty_education.Count>0?data.jntuh_registered_faculty_education.Select(e => e.educationId).Max():0;


                        if (data.Absent == true)
                        {
                            Reason1 = "Absent" + ",";
                        }
                        if (data.NotQualifiedAsperAICTE == true || degreeid < 4)
                        {
                            Reason1 += "NOT QUALIFIED " + ",";
                        }
                        if (data.NoSCM == true)
                        {
                            Reason1 += "NO SCM" + ",";
                        }
                        if (string.IsNullOrEmpty(data.PANNumber))
                        {
                            Reason1 += "NO PAN" + ",";
                        }
                        if (data.DepartmentId == null)
                        {
                            Reason1 += "No Department" + ",";
                        }
                        if (data.PHDundertakingnotsubmitted == true)
                        {
                            Reason1 += "No Undertaking" + ",";
                        }
                        if (data.Blacklistfaculy == true)
                        {
                            Reason1 += "Blacklisted" + ",";
                        }
                        if (data.FalsePAN == true)
                        {
                            Reason1 += "False Pan" + ",";
                        }
                        if (Reason1 != "")
                        {
                            Reason1 = Reason1.Substring(0, Reason1.Length - 1);
                            faculty.Reason = Reason1;
                        }


                        faculty.CollegeFacultiesliList = new List<CollegeFaculty>();
                        var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                        if (collegeFacultyWithIntakeReport != null)
                        {
                            collegeFacultyWithIntakeReport.LabsListDefs1.Add(labs);
                            collegeFacultyWithIntakeReport.CollegeFaculties.Add(faculty);
                            collegeFacultyWithIntakeReport.CollegeIntakeExistings.Add(intake);
                            collegeFacultyWithIntakeReport.CollegeIntakeSupportingDocuments.Add(intakesupportingDocuments);
                        }

                    }
                }
                else
                {
                    var faculty = new CollegeFaculty();
                    var labs = new AnonymousLabclass();
                    var intake = new CollegeIntakeExisting();
                    var intakesupportingDocuments = new CollegeIntakeExisting();
                    faculty.CollegeFacultiesliList = new List<CollegeFaculty>();
                    var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                    if (collegeFacultyWithIntakeReport != null)
                    {
                        collegeFacultyWithIntakeReport.CollegeFaculties.Add(faculty);
                        collegeFacultyWithIntakeReport.LabsListDefs1.Add(labs);
                        collegeFacultyWithIntakeReport.CollegeIntakeExistings.Add(intake);
                        collegeFacultyWithIntakeReport.CollegeIntakeSupportingDocuments.Add(intakesupportingDocuments);
                    }

                }
            #endregion

                #region Principal
                var Reason = string.Empty;
                if (principalDetails.Count > 0)
                {
                    var item = principalDetails.FirstOrDefault(i => i.collegeId == collegeId);

                    var faculty = new CollegeFaculty();
                    if (item != null)
                    {
                        var data = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();

                        faculty.FacultyRegistrationNumber = item.RegistrationNumber;
                        faculty.collegeId = item.collegeId;
                        faculty.id = data.id;
                        faculty.CollegeName = jntuh_college.Where(i => i.id == item.collegeId).Select(i => i.collegeName).FirstOrDefault();
                        faculty.facultyFirstName = data.FirstName;
                        faculty.facultyLastName = data.LastName;
                        faculty.facultySurname = data.MiddleName;
                        faculty.ViewNotificationDocument = item.NOtificationReport;
                        faculty.ViewAppointmentOrderDocument = item.AppointMentOrder;
                        faculty.ViewJoiningReportDocument = item.JoiningOrder;
                        faculty.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                        faculty.ViewPhdUndertakingDocument = item.PHDUndertakingDocument;
                        faculty.ViewPhysicalPresenceDocument = item.PhysicalPresenceonInspection;

                        if (data.Absent == true)
                        {
                            Reason = "NOT AVAILABLE" + ",";
                        }
                        if (data.NotQualifiedAsperAICTE == true)
                        {
                            Reason += "NOT QUALIFIED " + ",";
                        }
                        if (data.InvalidPANNumber == true)
                        {
                            Reason += "NO PAN" + ",";
                        }
                        if (data.FalsePAN == true)
                        {
                            Reason += "FALSE PAN" + ",";
                        }
                        if (data.NoSCM == true)
                        {
                            Reason += "NO SCM/RATIFICATION" + ",";
                        }
                        if (data.IncompleteCertificates == true)
                        {
                            Reason += "Incomplete Certificates" + ",";
                        }
                        if (data.PHDundertakingnotsubmitted == true)
                        {
                            Reason += "No Undertaking" + ",";
                        }
                        if (data.Blacklistfaculy == true)
                        {
                            Reason += "Blacklisted" + ",";
                        }
                        if (Reason != "")
                        {
                            Reason = Reason.Substring(0, Reason.Length - 1);
                            faculty.Reason = Reason;
                        }






                        var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                        if (collegeFacultyWithIntakeReport != null)
                        {
                            var collegeFaculty = collegeFacultyWithIntakeReport.CollegeFaculties.FirstOrDefault();
                            if (collegeFaculty != null)
                            {
                                var firstOrDefault = collegeFaculty.CollegeFacultiesliList;
                                if (firstOrDefault != null)
                                    firstOrDefault.Add(faculty);
                            }
                        }
                    }
                }
                #endregion

                #region For labs
                var collegeLabAnonymousLabclass = new List<AnonymousLabclass>();
                var equipids = db.jntuh_appeal_college_laboratories.Where(e => e.CollegeID == collegeId).Select(e => e.EquipmentID).Distinct().ToArray();

                var CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

                if (CollegeAffiliationStatus == "Yes")
                {
                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                         .Where(l => equipids.Contains(l.id))
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

                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                       .Where(l => equipids.Contains(l.id) && l.Labcode != "TMP-CL")
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


                var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                if (facultyDetails.Count > 0)
                {
                    var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                    if (collegeFacultyWithIntakeReport != null)
                        collegeFacultyWithIntakeReport.LabsListDefs1 = list1.ToList();
                }

                #endregion

                #region Intake
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();
                var collegeIntakeExisting = new List<CollegeIntakeExisting>();
                var collegeIntakes = db.jntuh_appeal_college_intake_existing.AsNoTracking().Where(i => i.collegeId == collegeId).ToList();
                var presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                var actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

                var AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                var AYY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                var AYY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                var AYY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                var AYY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                var AYY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                foreach (var item in collegeIntakes)
                {
                    var newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.isActive = item.isActive;
                    newIntake.nbaFrom = item.nbaFrom;
                    newIntake.nbaTo = item.nbaTo;
                    newIntake.specializationId = item.specializationId;
                    var jntuhSpecialization = jntuh_specialization.FirstOrDefault(i => i.id == item.specializationId);
                    if (jntuhSpecialization != null)
                    {
                        newIntake.Specialization = jntuhSpecialization.specializationName;
                        newIntake.DepartmentID = jntuhSpecialization.departmentId;
                        newIntake.Department = jntuh_department.FirstOrDefault(i => i.id == jntuhSpecialization.departmentId).departmentName;
                        newIntake.degreeID = jntuh_department.FirstOrDefault(i => i.id == jntuhSpecialization.departmentId).degreeId;
                        newIntake.Degree = jntuh_degree.FirstOrDefault(i => i.id == newIntake.degreeID).degree;
                    }

                    newIntake.AICTEApprovalLettr = item.IntakeApprovalLetter != null ? item.IntakeApprovalLetter + ".pdf" : "";
                    newIntake.shiftId = item.shiftId;
                    collegeIntakeExisting.Add(newIntake);
                }
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

                var intakelists = new List<CollegeIntakeExisting>();
                foreach (var item in collegeIntakeExisting)
                {

                    if (item.nbaFrom != null)
                        item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                    if (item.nbaTo != null)
                        item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

                    //FLAG : 1 - Approved, 0 - Admitted
                    var details = db.jntuh_appeal_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == AY0 && e.specializationId == item.specializationId)
                                                              .Select(e => e)
                                                              .FirstOrDefault();
                    if (details != null)
                    {
                        item.ApprovedIntake = details.approvedIntake;
                        item.letterPath = details.approvalLetter;
                        item.ProposedIntake = details.proposedIntake;
                        item.courseStatus = details.courseStatus;
                    }

                    item.approvedIntake1 = GetIntake(collegeId, AYY1, item.specializationId, item.shiftId, 1);
                    item.admittedIntake1 = GetIntake(collegeId, AYY1, item.specializationId, item.shiftId, 0);

                    item.approvedIntake2 = GetIntake(collegeId, AYY2, item.specializationId, item.shiftId, 1);
                    item.admittedIntake2 = GetIntake(collegeId, AYY2, item.specializationId, item.shiftId, 0);

                    item.approvedIntake3 = GetIntake(collegeId, AYY3, item.specializationId, item.shiftId, 1);
                    item.admittedIntake3 = GetIntake(collegeId, AYY3, item.specializationId, item.shiftId, 0);

                    item.approvedIntake4 = GetIntake(collegeId, AYY4, item.specializationId, item.shiftId, 1);
                    item.admittedIntake4 = GetIntake(collegeId, AYY4, item.specializationId, item.shiftId, 0);

                    item.approvedIntake5 = GetIntake(collegeId, AYY5, item.specializationId, item.shiftId, 1);
                    item.admittedIntake5 = GetIntake(collegeId, AYY5, item.specializationId, item.shiftId, 0);

                    intakelists.Add(item);


                }

                facultyDetails.FirstOrDefault().CollegeIntakeExistings = intakelists.Count > 0 ? intakelists.Where(i => i.shiftId == 1).OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList() : new List<CollegeIntakeExisting>();
                #endregion

                #region IntakeSuppportingDocuments

                var supportingDocuments = db.jntuh_appeal_college_intake_existing_supportingdocuments.AsNoTracking().Where(i => i.collegeId == collegeId).ToList();
                var collegeIntakeSupportingDocs = new List<CollegeIntakeExisting>();
                foreach (var item in supportingDocuments)
                {
                    var newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.specializationId = item.specializationId;
                    var jntuhSpecialization = jntuh_specialization.FirstOrDefault(i => i.id == item.specializationId);
                    if (jntuhSpecialization != null)
                    {
                        newIntake.Specialization = jntuhSpecialization.specializationName;
                        newIntake.DepartmentID = jntuhSpecialization.departmentId;
                        newIntake.Department = jntuh_department.FirstOrDefault(i => i.id == jntuhSpecialization.departmentId).departmentName;
                        newIntake.degreeID = jntuh_department.FirstOrDefault(i => i.id == jntuhSpecialization.departmentId).degreeId;
                        newIntake.Degree = jntuh_degree.FirstOrDefault(i => i.id == newIntake.degreeID).degree;
                    }

                    newIntake.ViewSCMApprovalLetter = item.SCM != null ? item.SCM + ".pdf" : "";
                    newIntake.ViewForm16ApprovalLetter = item.FORM16 != null ? item.FORM16 + ".pdf" : "";
                    newIntake.shiftId = item.shiftId;
                    collegeIntakeSupportingDocs.Add(newIntake);
                }
                collegeIntakeSupportingDocs = collegeIntakeSupportingDocs.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();


                facultyDetails.FirstOrDefault().CollegeIntakeSupportingDocuments = collegeIntakeSupportingDocs.Count > 0 ? collegeIntakeSupportingDocs.Where(i => i.shiftId == 1).OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList() : new List<CollegeIntakeExisting>();
                #endregion

                #region Remarks&OthersupportingDocs

                var collegeeditdetails = db.jntuh_appeal_college_edit_status.AsNoTracking().FirstOrDefault(i => i.collegeId == collegeId);

                if (collegeeditdetails != null && collegeeditdetails.DeclarationPath != null)
                {
                    facultyDetails.FirstOrDefault().DeclarationPath = collegeeditdetails.DeclarationPath;
                    facultyDetails.FirstOrDefault().Remarks = collegeeditdetails.Remarks;
                    facultyDetails.FirstOrDefault().FurtherAppealSupportingDoc = collegeeditdetails.FurtherAppealSupportingDocument;
                }

                #endregion
            }
            return View(facultyDetails);
        }


        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult ViewLabDetails(int? id, int collegeId, int? eqpid, int? eqpno)
        {

            Lab laboratories = new Lab();
            laboratories.collegeId = collegeId;

            if (id != null)
            {
                ViewBag.IsUpdate = true;
                laboratories = (from m in db.jntuh_lab_master
                                join labs in db.jntuh_appeal_college_laboratories on m.id equals labs.EquipmentID
                                where (labs.CollegeID == collegeId && labs.id == id)
                                select new Lab
                                {
                                    id = labs.id,
                                    collegeId = collegeId,
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
                    return PartialView("_ViewLabsDetails", laboratories);
                }

            }

            return RedirectToAction("CollegeAppealDetails", new { collegeid = collegeId });
        }


        #endregion


        #region OldRequirement

       


        //Appeal Principal Details
        [Authorize(Roles = "Admin")]
        public ActionResult ViewAppealPrincipalDetails()
        {
            List<CollegeFaculty> principalFaculty = new List<CollegeFaculty>();
            var principalDetails = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
            var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            if (principalDetails != null)
            {

                foreach (var item in principalDetails)
                {
                    CollegeFaculty faculty = new CollegeFaculty();
                    faculty.FacultyRegistrationNumber = item.RegistrationNumber;
                    faculty.collegeId = item.collegeId;
                    faculty.id = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.id).FirstOrDefault();
                    faculty.CollegeName = jntuh_college.Where(i => i.id == item.collegeId).Select(i => i.collegeName).FirstOrDefault();
                    faculty.facultyFirstName = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.FirstName).FirstOrDefault();
                    faculty.facultyLastName = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.LastName).FirstOrDefault();
                    faculty.facultySurname = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.MiddleName).FirstOrDefault();
                    faculty.ViewNotificationDocument = item.NOtificationReport;
                    faculty.ViewAppointmentOrderDocument = item.AppointMentOrder;
                    faculty.ViewJoiningReportDocument = item.JoiningOrder;
                    faculty.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                    faculty.ViewPhdUndertakingDocument = item.PHDUndertakingDocument;
                    faculty.ViewPhysicalPresenceDocument = item.PhysicalPresenceonInspection;
                    principalFaculty.Add(faculty);
                }

            }

            return View(principalFaculty);
        }


        //View Appeal Supporting Documents
        public ActionResult ViewSupportingDocuments(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            int[] CollegeIds = db.jntuh_appeal_college_intake_existing_supportingdocuments.GroupBy(e => e.collegeId).Select(e => e.Key).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive == true && CollegeIds.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            if (collegeId != null)
            {
                var AppealIntakesupportingDocuments = db.jntuh_appeal_college_intake_existing_supportingdocuments.Where(e => e.collegeId == collegeId).ToList();
                var jntuh_specialization = db.jntuh_specialization.AsEnumerable().ToList();
                var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();
                var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
                if (AppealIntakesupportingDocuments != null)
                {
                    foreach (var item in AppealIntakesupportingDocuments)
                    {
                        CollegeIntakeExisting newintake = new CollegeIntakeExisting();
                        newintake.specializationId = item.specializationId;
                        newintake.Specialization = jntuh_specialization.FirstOrDefault(e => e.id == item.specializationId).specializationName;
                        newintake.DepartmentID = jntuh_specialization.FirstOrDefault(e => e.id == item.specializationId).departmentId;
                        newintake.Department = jntuh_department.FirstOrDefault(e => e.id == newintake.DepartmentID).departmentName;
                        newintake.degreeID = jntuh_department.FirstOrDefault(e => e.id == newintake.DepartmentID).degreeId;
                        newintake.Degree = jntuh_degree.FirstOrDefault(e => e.id == newintake.degreeID).degree;
                        newintake.shiftId = item.shiftId;
                        newintake.ViewSCMApprovalLetter = item.SCM;
                        newintake.ViewForm16ApprovalLetter = item.FORM16;
                        collegeIntakeExisting.Add(newintake);
                    }
                    // collegeIntakeExisting = collegeIntakeExisting.GroupBy(e => new { e.specializationId, e.shiftId }).Select(e => e).ToList();

                }
                return View(collegeIntakeExisting);
            }
            else
            {
                return View(new List<CollegeIntakeExisting>());
            }

        }

        #endregion


        #region appeal Compliance and Reverification Faculty Details
        //Appeal Pdf Reports
        [Authorize(Roles = "College,Admin")]
        public ActionResult AppealComplianceData(int preview, string strcollegeId)
        {
            serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strcollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            string pdfPath = string.Empty;
            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            if (preview == 0)
            {
                pdfPath = SaveAppealCompliance(preview, collegeId);
                pdfPath = pdfPath.Replace("/", "\\");

            }
            //return File(pdfPath, "application/pdf", "A-114-" + collegeCode + ".pdf");
            return File(pdfPath, "application/pdf", collegeCode + "_AppealPdfReport.pdf");


        }

        private string SaveAppealCompliance(int preview, int collegeId)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/AppealPdfReports/");

            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            string collegeName = db.jntuh_college.Find(collegeId).collegeName;

            if (preview == 0)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //fullPath = path + "/temp/A-114_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                fullPath = path + collegeCode + "_" + collegeName.Substring(0, 3) + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";

                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeName = collegeName;
                iTextEvents.CollegeCode = collegeCode;
                //iTextEvents.formType = "A-114";
                iTextEvents.formType = "Acknowledgement";
                pdfWriter.PageEvent = iTextEvents;

            }

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path        
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/AppealPdfReport.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);
            contents = collegeInformation(collegeId, contents);
            contents = PrincipalDirectorDetails(collegeId, contents);
            contents = collegeFacultyComplianceMembers(collegeId, contents);
            contents = collegeFacultyReverificationMembers(collegeId, contents);
            contents = LaboratoriesDetails(collegeId, contents);
            contents = ExistingIntakeDetails(collegeId, contents);
            contents = Remarks(collegeId, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;

            foreach (var htmlElement in parsedHtmlElements)
            {
                if (htmlElement.Equals("<textarea>"))
                {
                    pdfDoc.NewPage();
                }

                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pdfDoc.SetMargins(60, 50, 60, 60);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = false;
                        }
                    }

                    pdfDoc.NewPage();

                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        private string collegeInformation(int collegeId, string contents)
        {
            CollegeInformation collegeInformation = new CollegeInformation();

            #region from jntuh_college table
            jntuh_college collegeDetails = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                                           .FirstOrDefault();
            if (collegeDetails != null)
            {
                collegeInformation.collegeName = collegeDetails.collegeName;
                collegeInformation.collegeCode = collegeDetails.collegeCode;
                collegeInformation.eamcetCode = collegeDetails.eamcetCode;
                collegeInformation.icetCode = collegeDetails.icetCode;
            }
            contents = contents.Replace("##AUDITSCHEDULECOLLEGENAME##", collegeInformation.collegeName);
            contents = contents.Replace("##COLLEGE_NAME##", collegeInformation.collegeName);
            contents = contents.Replace("##COLLEGE_CODE##", collegeInformation.collegeCode);
            contents = contents.Replace("##EAMCET_CODE##", collegeInformation.eamcetCode);
            contents = contents.Replace("##ICET_CODE##", collegeInformation.icetCode);
            //   barcodetext += "College Code:" + collegeInformation.collegeCode + ";College Name:" + collegeInformation.collegeName;
            //  LatefeeQrCodetext += "College Code:" + collegeInformation.collegeCode + ";College Name:" + collegeInformation.collegeName;
            string strCollegeType = string.Empty;
            List<jntuh_college_type> collegeType = db.jntuh_college_type.Where(s => s.isActive == true).ToList();
            foreach (var item in collegeType)
            {
                string YesOrNo = "no_b";
                int existCollegeTypeId = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                                         .Select(college => college.collegeTypeID)
                                                         .FirstOrDefault();
                if (item.id == existCollegeTypeId)
                {
                    YesOrNo = "yes_b";

                    strCollegeType += string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1} &nbsp; &nbsp; &nbsp;", YesOrNo, item.collegeType);
                }
            }

            contents = contents.Replace("##COLLEGE_TYPE##", strCollegeType);

            string strCollegeStatus = string.Empty;
            List<jntuh_college_status> jntuh_college_status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
            foreach (var item in jntuh_college_status)
            {
                string YesOrNo = "no_b";
                int existCollegeStatusId = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                                         .Select(college => college.collegeStatusID)
                                                         .FirstOrDefault();
                if (item.id == existCollegeStatusId)
                    YesOrNo = "yes_b";

                strCollegeStatus += string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1} &nbsp; &nbsp; &nbsp;", YesOrNo, item.collegeStatus);
            }
            contents = contents.Replace("##College_Status##", strCollegeStatus);

            #endregion

            #region from jntuh_address table

            jntuh_address addressDetails = db.jntuh_address.Where(address => address.collegeId == collegeId && address.addressTye == "COLLEGE")
                                                           .FirstOrDefault();
            string state = string.Empty;
            string district = string.Empty;
            if (addressDetails != null)
            {
                collegeInformation.address = addressDetails.address;
                collegeInformation.townOrCity = addressDetails.townOrCity;
                collegeInformation.mandal = addressDetails.mandal;
                collegeInformation.pincode = addressDetails.pincode;
                collegeInformation.fax = addressDetails.fax;
                collegeInformation.landline = addressDetails.landline;
                collegeInformation.mobile = addressDetails.mobile;
                collegeInformation.email = addressDetails.email;
                collegeInformation.website = addressDetails.website;
                state = db.jntuh_state.Where(s => s.isActive == true && s.id == addressDetails.stateId).Select(s => s.stateName).FirstOrDefault();
                district = db.jntuh_district.Where(d => d.isActive == true && d.id == addressDetails.districtId).Select(d => d.districtName).FirstOrDefault();
            }
            contents = contents.Replace("##COLLEGE_ADDRESS##", collegeInformation.address);
            contents = contents.Replace("##COLLEGE_City/Town##", collegeInformation.townOrCity);
            contents = contents.Replace("##COLLEGE_Mandal##", collegeInformation.mandal);
            contents = contents.Replace("##COLLEGE_District##", district);
            contents = contents.Replace("##COLLEGE_State##", state);
            contents = contents.Replace("##COLLEGE_Pincode##", collegeInformation.pincode.ToString() == "0" ? "" : collegeInformation.pincode.ToString());
            contents = contents.Replace("##COLLEGE_Fax##", collegeInformation.fax);
            contents = contents.Replace("##COLLEGE_Landline##", collegeInformation.landline);
            contents = contents.Replace("##COLLEGE_Mobile##", collegeInformation.mobile);
            contents = contents.Replace("##COLLEGE_Email##", collegeInformation.email);
            contents = contents.Replace("##COLLEGE_Website##", collegeInformation.website);

            #endregion

            #region from jntuh_college_affiliation table
            int NACId = 0;
            string affiliationNAAC = string.Empty;
            int affiliationNAACId = 0;
            string affiliationNAACFromDate = string.Empty;
            string affiliationNAACToDate = string.Empty;
            string affiliationNAACYes = string.Empty;
            string affiliationNAACNo = string.Empty;
            string affiliationNAACGrade = string.Empty;
            string affiliationNAACCGPA = string.Empty;
            string collegeAffiliationType = string.Empty;
            string yes = "no_b";
            string no = "no_b";
            List<jntuh_affiliation_type> affiliationType = db.jntuh_affiliation_type.OrderBy(a => a.id).Where(a => a.isActive == true).ToList();
            foreach (var item in affiliationType)
            {
                if (item.affiliationType.Trim() == "NAAC")
                {
                    affiliationNAAC = item.affiliationType.Trim();
                    affiliationNAACId = item.id;
                }
                else
                {
                    if (item.affiliationType.Trim() == "NBA Status")
                    {
                        collegeAffiliationType += "<tr>";
                        collegeAffiliationType += "<td valign='top' colspan='4'>" + item.affiliationType + "</td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>";
                        collegeAffiliationType += "##AFFILIATIONTYPEIMAGE" + item.id + "##";
                        collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred, Period </td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>From:##AFFILIATIONTYPEFROMDATE" + item.id + "## <br/>";
                        collegeAffiliationType += "Duration:##AFFILIATIONTYPEDURATION" + item.id + "##</td>";
                        collegeAffiliationType += "</tr>";
                        collegeAffiliationType += "<br />";
                        NACId = item.id;
                    }
                    else
                    {
                        collegeAffiliationType += "<tr>";
                        collegeAffiliationType += "<td valign='top' colspan='4'>" + item.affiliationType + "</td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>";
                        collegeAffiliationType += "##AFFILIATIONTYPEIMAGE" + item.id + "##";
                        collegeAffiliationType += "<td valign='top' colspan='4'>If Yes, Period </td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>From:##AFFILIATIONTYPEFROMDATE" + item.id + "## <br/>";
                        collegeAffiliationType += "To:##AFFILIATIONTYPETODATE" + item.id + "## <br/>";
                        collegeAffiliationType += "Duration:##AFFILIATIONTYPEDURATION" + item.id + "##</td>";
                        collegeAffiliationType += "</tr>";
                        collegeAffiliationType += "<br />";
                    }
                }
            }

            List<jntuh_college_affiliation> affiliationTypeDetails = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId).ToList();
            foreach (var affiliation in affiliationTypeDetails)
            {
                if (affiliationNAACId == affiliation.affiliationTypeId)
                {
                    affiliationNAACYes = "yes_b";
                    affiliationNAACNo = "no_b";
                    if (affiliation.affiliationGrade != null)
                    {
                        affiliationNAACGrade = affiliation.affiliationGrade;
                    }
                    if (affiliation.CGPA != null)
                    {
                        affiliationNAACCGPA = affiliation.CGPA;
                    }
                    if (affiliation.affiliationFromDate != null && affiliation.affiliationToDate != null)
                    {
                        string fromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        string toDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                        affiliationNAACFromDate = fromDate;
                        affiliationNAACToDate = ((Convert.ToInt32(toDate.Substring(toDate.Length - 4))) - (Convert.ToInt32(fromDate.Substring(fromDate.Length - 4)))).ToString();
                    }
                }
                if (affiliation.affiliationTypeId == NACId)
                {
                    string image = string.Empty;
                    if (affiliation.affiliationFromDate != null)
                    {
                        string fDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + affiliation.affiliationTypeId + "##", fDate);
                        if (affiliation.affiliationToDate != null)
                        {
                            string duration = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", ((Convert.ToInt32(duration.Substring(duration.Length - 4))) - (Convert.ToInt32(fDate.Substring(fDate.Length - 4)))).ToString());
                        }
                        else
                        {
                            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", affiliation.affiliationDuration.ToString());
                        }
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", image);
                    }
                    else if (affiliation.affiliationStatus == "Applied")
                    {
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", image);
                    }
                }
                else
                {
                    if (affiliation.affiliationFromDate != null && affiliation.affiliationToDate != null)
                    {
                        yes = "yes_b";
                        no = "no_b";
                        string fDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        string tDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                        string duration = affiliation.affiliationDuration.ToString();
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + affiliation.affiliationTypeId + "##", fDate);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPETODATE" + affiliation.affiliationTypeId + "##", tDate);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", duration);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1} &nbsp;&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_{2}.png' height='10' />&nbsp;{3}", yes, "Yes", no, "No</td>"));
                    }
                }


            }
            foreach (var item in affiliationType)
            {
                if (item.affiliationType.Trim() == "NBA Status")
                {
                    string image = string.Empty;
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + item.id + "##", string.Empty);
                    image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + item.id + "##", image);
                }
                else
                {
                    yes = "no_b";
                    no = "yes_b";
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPETODATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + item.id + "##", string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1} &nbsp;&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_{2}.png' height='10' />&nbsp;{3}", yes, "Yes", no, "No</td>"));
                }
            }
            if (affiliationNAAC == "NAAC")
            {
                string image = string.Empty;
                int nackid = db.jntuh_affiliation_type.Where(at => at.affiliationType == "NAAC").Select(at => at.id).FirstOrDefault();

                var nackatype = db.jntuh_college_affiliation.Where(at => at.affiliationTypeId == nackid && at.collegeId == collegeId).Select(at => at).FirstOrDefault();
                if (nackatype != null)
                {
                    if (nackatype.affiliationFromDate != null && nackatype.affiliationToDate != null)
                    {
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                    else if (nackatype.affiliationStatus == "Applied")
                    {
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                    else
                    {
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                }
                else
                {
                    image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");

                }
                collegeAffiliationType += "<tr>";
                if (nackatype != null)
                {
                    collegeAffiliationType += "<td valign='top' colspan='4'>NAAC</td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>";
                    collegeAffiliationType += image;
                    collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred,Period </td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>From: " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(nackatype.affiliationFromDate.ToString()).ToString() + "<br/>";
                    collegeAffiliationType += "To:&nbsp; " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(nackatype.affiliationToDate.ToString()) + "<br/>";
                    collegeAffiliationType += "Duration: " + nackatype.affiliationDuration + "<br/>";
                    collegeAffiliationType += "Grade: " + nackatype.affiliationGrade + "<br/>";
                    collegeAffiliationType += "CGPA: " + nackatype.CGPA + "</td>";
                }
                else
                {
                    collegeAffiliationType += "<td valign='top' colspan='4'>NAAC</td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>";
                    collegeAffiliationType += image;
                    collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred,Period </td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>From: <br/>";
                    collegeAffiliationType += "To:&nbsp; <br/>";
                    collegeAffiliationType += "Duration: <br/>";
                    collegeAffiliationType += "Grade:<br/>";
                    collegeAffiliationType += "CGPA: </td>";
                }

                collegeAffiliationType += "</tr>";
                collegeAffiliationType += "<br />";
            }
            contents = contents.Replace("##COLLEGE_AFFILIATIONTYPES##", collegeAffiliationType);

            #endregion

            #region from jntuh_college_degree table

            string strCollegeDegree = string.Empty;
            strCollegeDegree += "<table border='0' cellspacing='0' cellpadding='0'><tbody><tr>";
            List<jntuh_degree> collegeDegree = db.jntuh_degree.OrderBy(degree => degree.degreeDisplayOrder).Where(degree => degree.isActive == true).ToList();
            int count = 0;
            foreach (var item in collegeDegree)
            {
                strCollegeDegree += "<td width='10%'>" + string.Format("{0}&nbsp; {1}", "##COLLEGEDEGREEIMAGE" + item.id + "##", item.degree) + "</td>";
                count++;
                if (count % 5 == 0)
                {
                    strCollegeDegree += "</tr>";
                }
            }
            List<jntuh_college_degree> collegeDegrees = db.jntuh_college_degree.Where(degree => degree.isActive == true && degree.collegeId == collegeId).ToList();
            foreach (var degrees in collegeDegrees)
            {
                strCollegeDegree = strCollegeDegree.Replace("##COLLEGEDEGREEIMAGE" + degrees.degreeId + "##", string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />", "yes_b"));
            }
            foreach (var item in collegeDegree)
            {
                strCollegeDegree = strCollegeDegree.Replace("##COLLEGEDEGREEIMAGE" + item.id + "##", string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />", "no_b"));
            }
            strCollegeDegree += "</tbody></table>";
            contents = contents.Replace("##COLLEGE_DEGREE##", strCollegeDegree);
            #endregion
            return contents;
        }

        public string PrincipalDirectorDetails(int collegeId, string contents)
        {
            // collegeId = 72;
            string strPrincipal = string.Empty;
            ////Principal Details
            var regNo = db.jntuh_appeal_principal_registered.Where(r => r.collegeId == collegeId).Select(r => r.RegistrationNumber).FirstOrDefault();
            if (!string.IsNullOrEmpty(regNo))
            {
                var PrincipalDetails = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == regNo).Select(r => r).FirstOrDefault();

                if (PrincipalDetails != null)
                {
                    var education = db.jntuh_registered_faculty_education.Where(e => e.facultyId == PrincipalDetails.id).OrderByDescending(e => e.id).Select(e => e).FirstOrDefault();

                    strPrincipal += "<p><strong><u>1.Details of Principal</u></strong></p><br />";
                    strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                    strPrincipal += "<tbody>";
                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.RegistrationNumber + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.FirstName + "</td>";
                    strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.MiddleName + "</td>";
                    strPrincipal += "</tr>";
                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.LastName + "</td>";
                    //strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //if (education != null)
                    //{
                    //    if (education.courseStudied != null)
                    //    {
                    //        strPrincipal += "<td valign='top' colspan='5'>" + education.courseStudied + "</td>";
                    //    }
                    //    else
                    //    {
                    //        strPrincipal += "<td valign='top' colspan='5'></td>";
                    //    }
                    //}
                    //else
                    //{
                    //    strPrincipal += "<td valign='top' colspan='5'></td>";
                    //}
                    //strPrincipal += "</tr>";

                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                    //if (education != null)
                    //{
                    //    if (education.specialization != null)
                    //    {
                    //        strPrincipal += "<td valign='top' colspan='5'>" + education.specialization + "</td>";
                    //    }
                    //    else
                    //    {
                    //        strPrincipal += "<td valign='top' colspan='5'></td>";
                    //    }
                    //}
                    //else
                    //{
                    //    strPrincipal += "<td valign='top' colspan='5'></td>";
                    //}
                    //strPrincipal += "<td valign='top' colspan='4' style='font-size: 8px;'>Date of Appointment </td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                    //strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.DateOfAppointment + "</td>";
                    //strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.DateOfBirth + "</td>";
                    strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.LastName + "</td>";

                    strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.Mobile + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.Email + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Ratified</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //if (PrincipalDetails.isFacultyRatifiedByJNTU == true)
                    //{
                    //    strPrincipal += "<td colspan='5' valign='top'>Yes</td>";
                    //}
                    //else
                    //{
                    //    strPrincipal += "<td colspan='5' valign='top'>No</td>";
                    //}
                    strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='left'>:</td>";
                    if (!string.IsNullOrEmpty(PrincipalDetails.Photo))
                    {
                        string strPrincipalPhoto = string.Empty;
                        string path = @"~/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo;
                        path = System.Web.HttpContext.Current.Server.MapPath(path);
                        if (System.IO.File.Exists(path))
                        {
                            strPrincipalPhoto = "<img src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='left'  height='50' />";
                        }
                        strPrincipal += "<td colspan='15' valign='top' style='height: 50px;'>" + strPrincipalPhoto + "</td>";
                        //strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'></td>";
                    }
                    else
                    {
                        strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'></td>";
                    }
                    strPrincipal += "</tr>";
                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                    strPrincipal += "<br />";

                }
                else
                {
                    strPrincipal += "<p><strong><u>Details of Principal:</u></strong> (PRINCIPAL DETAILS ARE NOT UPLOADED)</p><br />";
                    strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                    strPrincipal += "<tbody>";
                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "</tr>";
                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "</tr>";

                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "<td valign='top' colspan='4' style='font-size: 8px;'>Date of Appointment </td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";

                    strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='15'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'></td>";
                    strPrincipal += "</tr>";


                    strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Ratified</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td colspan='5' valign='top'></td>";

                    strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='left'>:</td>";
                    strPrincipal += "<td colspan='5' valign='top'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                    strPrincipal += "<br />";
                }

            }
            else
            {
                strPrincipal += "<p><strong><u>Details of Principal:</u></strong> (PRINCIPAL DETAILS ARE NOT UPLOADED)</p><br />";
                strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                strPrincipal += "<tbody>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                strPrincipal += "<td colspan='15' valign='top'></td>";
                strPrincipal += "</tr>";

                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'></td>";
                strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'></td>";
                strPrincipal += "</tr>";
                //strPrincipal += "<tr>";
                //strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                //strPrincipal += "<td valign='top' colspan='5'></td>";
                //strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                //strPrincipal += "<td valign='top' colspan='5'></td>";
                //strPrincipal += "</tr>";

                //strPrincipal += "<tr>";
                //strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                //strPrincipal += "<td valign='top' colspan='5'></td>";
                //strPrincipal += "<td valign='top' colspan='4' style='font-size: 8px;'>Date of Appointment </td>";
                //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                //strPrincipal += "<td valign='top' colspan='5'></td>";
                //strPrincipal += "</tr>";

                strPrincipal += "<tr>";
                //strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                //strPrincipal += "<td valign='top' colspan='5'></td>";
                strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'></td>";
                strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                strPrincipal += "<td valign='top' colspan='15'></td>";
                strPrincipal += "</tr>";

                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                strPrincipal += "<td colspan='15' valign='top'></td>";
                strPrincipal += "</tr>";


                strPrincipal += "<tr>";
                //strPrincipal += "<td valign='top' colspan='4'>Ratified</td>";
                //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                //strPrincipal += "<td colspan='5' valign='top'></td>";

                strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                strPrincipal += "<td valign='top' colspan='1' align='left'>:</td>";
                strPrincipal += "<td colspan='5' valign='top'></td>";
                strPrincipal += "</tr>";

                strPrincipal += "</tbody>";
                strPrincipal += "</table>";
                strPrincipal += "<br />";
            }

            contents = contents.Replace("##PRINCIPAL##", strPrincipal);
            return contents;
        }

        private string collegeFacultyComplianceMembers(int collegeId, string contents)
        {
            //  collegeId = 177;
            string collegeFaculty = string.Empty;
            int count = 1;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;

            string ratified = string.Empty;
            string[] strRegNoS = db.jntuh_appeal_faculty_registered.Where(e => e.collegeId == collegeId && e.NOtificationReport != null && e.AppointMentOrder != null).Select(e => e.RegistrationNumber).ToArray();




            #region TeachingFacultyLogic Begin
            //int Facultytype = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
            //List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == Facultytype && f.collegeId == collegeId)
            //    .OrderBy(f => f.facultyDepartmentId).ThenBy(f => f.facultyDesignationId).ThenBy(f => f.facultyFirstName).ToList();

            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

            //Commented By Srinivas.T   FacultyVerificationStatus
            //List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null && r.createdBy != 63809).Select(r => r).ToList();
            // List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == collegeId && r.existingFacultyId == null).Select(r => r).ToList();
            List<jntuh_appeal_faculty_registered> regFaculty = db.jntuh_appeal_faculty_registered.Where(e => e.collegeId == collegeId && e.NOtificationReport != null).Select(e => e).ToList();
            //var RegistredFacultyLog = db.jntuh_registered_faculty_log.AsNoTracking().Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks }).ToList();
            var jntuh_registered_facultys = db.jntuh_registered_faculty.AsNoTracking().Select(F => F).ToList();
            var jntuh_designation = db.jntuh_designation.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_spec = db.jntuh_specialization.AsNoTracking().ToList();

            foreach (var item in regFaculty)
            {
                CollegeFaculty collegeFacultynew = new CollegeFaculty();
                jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == item.RegistrationNumber.Trim()).FirstOrDefault();

                if (rFaculty != null)
                {
                    //if (rFaculty.RegistrationNumber == "9893-150415-173751")
                    //{

                    //}


                    //if (rFaculty.collegeId != null)
                    //{
                    //    collegeFaculty.collegeId = (int)rFaculty.collegeId;
                    //}
                    collegeFacultynew.id = rFaculty.id;
                    collegeFacultynew.TotalExperience = rFaculty.TotalExperience != null ? rFaculty.TotalExperience : 0;
                    collegeFacultynew.collegeId = collegeId;
                    //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
                    collegeFacultynew.facultyType = rFaculty.type;
                    collegeFacultynew.facultyFirstName = rFaculty.FirstName;
                    collegeFacultynew.facultyLastName = rFaculty.MiddleName;
                    collegeFacultynew.facultySurname = rFaculty.LastName;
                    collegeFacultynew.facultyGenderId = rFaculty.GenderId;
                    collegeFacultynew.facultyFatherName = rFaculty.FatherOrHusbandName;
                    collegeFacultynew.photo = rFaculty.Photo;
                    //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

                    if (rFaculty.DateOfBirth != null)
                        collegeFacultynew.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

                    //if (rFaculty.WorkingStatus == true)
                    //{

                    // rFaculty.DesignationId!=null? (int)rFaculty.DesignationId:0;
                    //collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
                    collegeFacultynew.facultyDesignationId = rFaculty.DesignationId != null ? (int)rFaculty.DesignationId : 0;
                    collegeFacultynew.designation = db.jntuh_designation.Where(d => d.id == collegeFacultynew.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                    collegeFacultynew.facultyOtherDesignation = rFaculty.OtherDesignation;
                    collegeFacultynew.facultyDepartmentId = rFaculty.DepartmentId != null ? (int)rFaculty.DepartmentId : 0;
                    collegeFacultynew.department = db.jntuh_department.Where(d => d.id == collegeFacultynew.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    collegeFacultynew.facultyOtherDepartment = rFaculty.OtherDepartment;
                    // }
                    collegeFacultynew.SpecializationName = item.SpecializationId != null ? jntuh_spec.FirstOrDefault(i => i.id == item.SpecializationId).specializationName : "";
                    collegeFacultynew.facultyEmail = rFaculty.Email;
                    collegeFacultynew.facultyMobile = rFaculty.Mobile;
                    collegeFacultynew.facultyPANNumber = rFaculty.PANNumber;
                    collegeFacultynew.facultyAadhaarNumber = rFaculty.AadhaarNumber;
                    collegeFacultynew.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
                    //  collegeFacultynew.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == rFaculty.RegistrationNumber);
                    collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfAppointment.ToString());

                    // collegeFacultynew.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
                    //  collegeFacultynew.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber).Select(F => F.Remarks).FirstOrDefault();
                    teachingFaculty.Add(collegeFacultynew);
                }
            }
            var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Where(C => C.collegeId == collegeId).Select(C => C).ToList();

            jntuh_college_faculty_registered eFaculty = null;
            var jntuh_designations = db.jntuh_designation.Select(d => new { d.id, d.designation }).ToList();
            var jntuh_departments = db.jntuh_department.Select(d => new { d.id, d.departmentName }).ToList();

            #endregion TeachingFacultyLogic End





            var facultyList = teachingFaculty.Select(e => e).ToList();
            //int[] DeptIDs = facultyList.Where(a => a.facultyDepartmentId != null && a.facultyType == "ExistFaculty").Select(d => d.facultyDepartmentId).Distinct().ToArray();
            int[] DeptIDs = facultyList.Select(d => d.facultyDepartmentId).Distinct().ToArray();


            if (DeptIDs.Count() > 0)
            {
                foreach (int deptId in DeptIDs)
                {
                    collegeFaculty += ComplianceFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).ToList());
                }
            }


            contents = contents.Replace("##COLLEGETeachingFaculty##", collegeFaculty);

            return contents;
        }


        private string collegeFacultyReverificationMembers(int collegeId, string contents)
        {
            // collegeId = 81;
            string collegeFaculty1 = string.Empty;
            int count = 1;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;

            string ratified = string.Empty;
            string[] strRegNoS = db.jntuh_appeal_faculty_registered.Where(e => e.collegeId == collegeId && e.AppealReverificationSupportingDocument != null).Select(e => e.RegistrationNumber).ToArray();




            #region TeachingFacultyLogic Begin


            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();


            List<jntuh_appeal_faculty_registered> regFaculty1 = db.jntuh_appeal_faculty_registered.Where(e => e.collegeId == collegeId && e.AppealReverificationSupportingDocument != null).Select(e => e).ToList();
            var jntuh_registered_facultys = db.jntuh_registered_faculty.AsNoTracking().Select(F => F).ToList();
            var jntuh_designation = db.jntuh_designation.AsNoTracking().ToList();

            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_spec = db.jntuh_specialization.AsNoTracking().ToList();

            foreach (var item in regFaculty1)
            {
                CollegeFaculty collegeFacultynew = new CollegeFaculty();
                jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == item.RegistrationNumber.Trim()).FirstOrDefault();

                if (rFaculty != null)
                {

                    collegeFacultynew.id = rFaculty.id;
                    collegeFacultynew.TotalExperience = rFaculty.TotalExperience != null ? rFaculty.TotalExperience : 0;
                    collegeFacultynew.collegeId = collegeId;
                    collegeFacultynew.facultyType = rFaculty.type;
                    collegeFacultynew.facultyFirstName = rFaculty.FirstName;
                    collegeFacultynew.facultyLastName = rFaculty.MiddleName;
                    collegeFacultynew.facultySurname = rFaculty.LastName;
                    collegeFacultynew.facultyGenderId = rFaculty.GenderId;
                    collegeFacultynew.facultyFatherName = rFaculty.FatherOrHusbandName;
                    collegeFacultynew.photo = rFaculty.Photo;

                    if (rFaculty.DateOfBirth != null)
                        collegeFacultynew.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());


                    collegeFacultynew.facultyDesignationId = rFaculty.DesignationId != null ? (int)rFaculty.DesignationId : 0;
                    collegeFacultynew.designation = jntuh_designation.Where(d => d.id == collegeFacultynew.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                    collegeFacultynew.facultyOtherDesignation = rFaculty.OtherDesignation;
                    collegeFacultynew.facultyDepartmentId = rFaculty.DepartmentId != null ? (int)rFaculty.DepartmentId : 0;
                    collegeFacultynew.department = jntuh_department.Where(d => d.id == collegeFacultynew.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    collegeFacultynew.facultyOtherDepartment = rFaculty.OtherDepartment;

                    collegeFacultynew.SpecializationName = item.SpecializationId != null ? jntuh_spec.FirstOrDefault(i => i.id == item.SpecializationId).specializationName : "";
                    collegeFacultynew.facultyEmail = rFaculty.Email;
                    collegeFacultynew.facultyMobile = rFaculty.Mobile;
                    collegeFacultynew.facultyPANNumber = rFaculty.PANNumber;
                    collegeFacultynew.facultyAadhaarNumber = rFaculty.AadhaarNumber;
                    collegeFacultynew.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
                    collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfAppointment.ToString());
                    teachingFaculty.Add(collegeFacultynew);
                }
            }
            var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Where(C => C.collegeId == collegeId).Select(C => C).ToList();

            jntuh_college_faculty_registered eFaculty = null;
            var jntuh_designations = db.jntuh_designation.Select(d => new { d.id, d.designation }).ToList();
            var jntuh_departments = db.jntuh_department.Select(d => new { d.id, d.departmentName }).ToList();

            #endregion TeachingFacultyLogic End





            var facultyList = teachingFaculty.Select(e => e).ToList();
            //int[] DeptIDs = facultyList.Where(a => a.facultyDepartmentId != null && a.facultyType == "ExistFaculty").Select(d => d.facultyDepartmentId).Distinct().ToArray();
            int[] DeptIDs = facultyList.Select(d => d.facultyDepartmentId).Distinct().ToArray();


            if (DeptIDs.Count() > 0)
            {
                foreach (int deptId in DeptIDs)
                {
                    collegeFaculty1 += ReverificationFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).ToList());
                }
            }


            contents = contents.Replace("##COLLEGEReverficationFaculty##", collegeFaculty1);

            return contents;
        }

        private string ComplianceFaculty(List<CollegeFaculty> facultyList)
        {
            int count = 1;
            string collegeFaculty = string.Empty;
            string ContentFaculty = string.Empty;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;
            string ratified = string.Empty;
            string strcheckList = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='8' />";

            //int? deptid = facultyList.Count() > 0 ? facultyList.FirstOrDefault().DepartmentId : null;

            int? deptid = null;
            if (facultyList.Count() > 0)
            {
                deptid = facultyList.FirstOrDefault().facultyDepartmentId;
            }
            else
                deptid = null;
            if (deptid == null || deptid == 0)
            {
                department = "New";
            }
            else
            {
                department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;
            }

            collegeFaculty += "<strong><u>" + department + "</u></strong> <br /> <br />";
            collegeFaculty += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            collegeFaculty += "<tbody>";
            collegeFaculty += "<tr>";
            collegeFaculty += "<td colspan='1'><p align='center'>SNo</p></td>";
            // collegeFaculty += "<td colspan='2'><p align='center'>Status</p></td>";
            collegeFaculty += "<td colspan='3'><p p align='left'>Faculty Name1</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Gender</p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Qualification </p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Specilization</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Identified for UG/PG/UG and PG</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Date of Appointment</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>EXPerience</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
            collegeFaculty += "<td colspan='4'><p align='center'>Registration Number</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Pan Number</p></td>";
            collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='3'><p align='center'>DeactivationReasons</p></td>";
            collegeFaculty += "</tr>";
            var jntuh_designations = db.jntuh_designation.Select(D => new { D.designation, D.id }).ToList();
            // var jntuh_registered_faculty_educations = db.jntuh_registered_faculty_education.ToList();
            var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Select(F => new { F.RegistrationNumber, F.collegeId, F.IdentifiedFor }).ToList();
            var RegistredFacultyLog = db.jntuh_registered_faculty_log.Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks }).ToList();
            var jntuh_registered_facultys = db.jntuh_registered_faculty.Select(F => new
            {
                F.RegistrationNumber,
                F.Photo,
                F.Absent,
                F.NotQualifiedAsperAICTE,
                F.jntuh_registered_faculty_education,
                F.NoSCM,
                F.PANNumber,
                F.FalsePAN,
                F.DepartmentId,
                F.PHDundertakingnotsubmitted,
                F.Blacklistfaculy

            }).ToList();

            foreach (var item in facultyList)
            {
                var Reason1 = "";
                if (item.facultyGenderId == 1)
                {
                    gender = "M";
                }
                else
                {
                    gender = "F";
                }

                designation = jntuh_designations.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                if (item.dateOfAppointment != null)
                {
                    dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.dateOfAppointment.ToString());
                }
                if (item.isFacultyRatifiedByJNTU == true)
                {
                    ratified = "Yes";
                }
                else
                {
                    ratified = "No";
                }

                qualification = db.jntuh_registered_faculty_education.OrderByDescending(education => education.educationId)
                                                         .Where(education => education.facultyId == item.id)
                                                         .Select(education => education.courseStudied).FirstOrDefault();

                var item1 = jntuh_registered_facultys.FirstOrDefault(i => i.RegistrationNumber.Trim() == item.FacultyRegistrationNumber.Trim());
                if (item1 != null)
                {
                    var degreeid = item1.jntuh_registered_faculty_education.Count > 0 ? item1.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0;
                    if (item1.Absent == true)
                    {
                        Reason1 = "Absent" + ",";
                    }
                    if ((item1.NotQualifiedAsperAICTE == true || degreeid < 4))
                    {
                        Reason1 += "NOT QUALIFIED " + ",";
                    }
                    if (item1.NoSCM == true)
                    {
                        Reason1 += "NO SCM" + ",";
                    }
                    if (string.IsNullOrEmpty(item1.PANNumber))
                    {
                        Reason1 += "NO PAN" + ",";
                    }
                    if (item1.FalsePAN == true)
                    {
                        Reason1 = "False PAN" + ",";
                    }
                    if (item1.DepartmentId == null)
                    {
                        Reason1 += "No Department" + ",";
                    }

                    if (item1.PHDundertakingnotsubmitted == true)
                    {
                        Reason1 += "No Undertaking" + ",";
                    }
                    if (item1.Blacklistfaculy == true)
                    {
                        Reason1 += "Blacklisted" + ",";
                    }
                    if (Reason1 != "")
                    {
                        Reason1 = Reason1.Substring(0, Reason1.Length - 1);
                    }
                }



                string identifiedfor = jntuh_college_faculty_registereds.Where(f => f.RegistrationNumber == item.FacultyRegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();
                string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == item.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.Remarks + "</p></td>";
                collegeFaculty += "<td colspan='3'><p p align='left'>" + (item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname).ToUpper() + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + gender + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + qualification + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + item.SpecializationName + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + identifiedfor + " </p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + dateOfAppointment + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.TotalExperience.ToString().Replace(".00", "") + "</p></td>";
                // collegeFaculty += "<td colspan='2'><p align='center'>" + ".00" + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultySalary + "</p></td>";
                collegeFaculty += "<td colspan='4'><p align='center'>" + item.FacultyRegistrationNumber + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPANNumber + "</p></td>";
                if (!string.IsNullOrEmpty(Facultyphoto))
                {
                    string strFacultyPhoto = string.Empty;
                    string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                    path = System.Web.HttpContext.Current.Server.MapPath(path);
                    if (System.IO.File.Exists(path.Trim()))
                    {
                        strFacultyPhoto = "<img src='" + serverURL + "/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                        collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                        //if(strFacultyPhoto!=null || strFacultyPhoto!="")
                        //{
                        //    collegeFaculty += "<td colspan='3'><p align='center'>" + "11-" + strFacultyPhoto.Trim() + "</p></td>";
                        //}
                        //else
                        //{
                        //    collegeFaculty += "<td colspan='3'><p align='center'>" + "111-" + strFacultyPhoto.Trim() + "</p></td>";
                        //}
                    }
                    else
                    {
                        collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                    }
                }
                else
                {
                    collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                }
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'><strong>" + Reason1 + "</strong></p></td>";
                collegeFaculty += "</tr>";
                count++;
            }
            collegeFaculty += "</tbody>";
            collegeFaculty += "</table>";
            return collegeFaculty;
        }

        private string ReverificationFaculty(List<CollegeFaculty> facultyList)
        {
            int count = 1;
            string collegeFaculty = string.Empty;
            string ContentFaculty = string.Empty;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;
            string ratified = string.Empty;
            string strcheckList = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='8' />";

            //int? deptid = facultyList.Count() > 0 ? facultyList.FirstOrDefault().DepartmentId : null;

            int? deptid = null;
            if (facultyList.Count() > 0)
            {
                deptid = facultyList.FirstOrDefault().facultyDepartmentId;
            }
            else
                deptid = null;
            if (deptid == null || deptid == 0)
            {
                department = "New";
            }
            else
            {
                department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;
            }

            collegeFaculty += "<strong><u>" + department + "</u></strong> <br /> <br />";
            collegeFaculty += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            collegeFaculty += "<tbody>";
            collegeFaculty += "<tr>";
            collegeFaculty += "<td colspan='1'><p align='center'>SNo</p></td>";
            // collegeFaculty += "<td colspan='2'><p align='center'>Status</p></td>";
            collegeFaculty += "<td colspan='3'><p p align='left'>Faculty Name1</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Gender</p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Qualification </p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Specilization</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Identified for UG/PG/UG and PG</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Date of Appointment</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>EXPerience</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
            collegeFaculty += "<td colspan='4'><p align='center'>Registration Number</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Pan Number</p></td>";
            collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
            collegeFaculty += "<td colspan='3'><p align='center'>DeactivationReasons</p></td>";
            collegeFaculty += "</tr>";
            var jntuh_designations = db.jntuh_designation.Select(D => new { D.designation, D.id }).ToList();
            // var jntuh_registered_faculty_educations = db.jntuh_registered_faculty_education.ToList();
            var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Select(F => new { F.RegistrationNumber, F.collegeId, F.IdentifiedFor }).ToList();
            var RegistredFacultyLog = db.jntuh_registered_faculty_log.Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks }).ToList();
            var jntuh_registered_facultys = db.jntuh_registered_faculty.Select(F => new
            {
                F.RegistrationNumber,
                F.Photo,
                F.Absent,
                F.NotQualifiedAsperAICTE,
                F.jntuh_registered_faculty_education,
                F.NoSCM,
                F.PANNumber,
                F.FalsePAN,
                F.DepartmentId,
                F.PHDundertakingnotsubmitted,
                F.Blacklistfaculy

            }).ToList();

            foreach (var item in facultyList)
            {
                var Reason1 = "";
                if (item.facultyGenderId == 1)
                {
                    gender = "M";
                }
                else
                {
                    gender = "F";
                }

                designation = jntuh_designations.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                if (item.dateOfAppointment != null)
                {
                    dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.dateOfAppointment.ToString());
                }
                if (item.isFacultyRatifiedByJNTU == true)
                {
                    ratified = "Yes";
                }
                else
                {
                    ratified = "No";
                }

                qualification = db.jntuh_registered_faculty_education.OrderByDescending(education => education.educationId)
                                                         .Where(education => education.facultyId == item.id)
                                                         .Select(education => education.courseStudied).FirstOrDefault();
                var item1 = jntuh_registered_facultys.FirstOrDefault(i => i.RegistrationNumber.Trim() == item.FacultyRegistrationNumber.Trim());
                if (item1 != null)
                {
                    var degreeid = item1.jntuh_registered_faculty_education.Count > 0 ? item1.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0;
                    if (item1.Absent == true)
                    {
                        Reason1 = "Absent" + ",";
                    }
                    if ((item1.NotQualifiedAsperAICTE == true || degreeid < 4))
                    {
                        Reason1 += "NOT QUALIFIED " + ",";
                    }
                    if (item1.NoSCM == true)
                    {
                        Reason1 += "NO SCM" + ",";
                    }
                    if (string.IsNullOrEmpty(item1.PANNumber))
                    {
                        Reason1 += "NO PAN" + ",";
                    }
                    if (item1.FalsePAN == true)
                    {
                        Reason1 = "False PAN" + ",";
                    }
                    if (item1.DepartmentId == null)
                    {
                        Reason1 += "No Department" + ",";
                    }

                    if (item1.PHDundertakingnotsubmitted == true)
                    {
                        Reason1 += "No Undertaking" + ",";
                    }
                    if (item1.Blacklistfaculy == true)
                    {
                        Reason1 += "Blacklisted" + ",";
                    }
                    if (Reason1 != "")
                    {
                        Reason1 = Reason1.Substring(0, Reason1.Length - 1);
                    }
                }


                string identifiedfor = jntuh_college_faculty_registereds.Where(f => f.RegistrationNumber == item.FacultyRegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();
                string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == item.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.Remarks + "</p></td>";
                collegeFaculty += "<td colspan='3'><p p align='left'>" + (item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname).ToUpper() + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + gender + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + qualification + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + item.SpecializationName + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + identifiedfor + " </p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + dateOfAppointment + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.TotalExperience.ToString().Replace(".00", "") + "</p></td>";
                // collegeFaculty += "<td colspan='2'><p align='center'>" + ".00" + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultySalary + "</p></td>";
                collegeFaculty += "<td colspan='4'><p align='center'>" + item.FacultyRegistrationNumber + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPANNumber + "</p></td>";
                if (!string.IsNullOrEmpty(Facultyphoto))
                {
                    string strFacultyPhoto = string.Empty;
                    string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                    path = System.Web.HttpContext.Current.Server.MapPath(path);
                    if (System.IO.File.Exists(path.Trim()))
                    {
                        strFacultyPhoto = "<img src='" + serverURL + "/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                        collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                        //if(strFacultyPhoto!=null || strFacultyPhoto!="")
                        //{
                        //    collegeFaculty += "<td colspan='3'><p align='center'>" + "11-" + strFacultyPhoto.Trim() + "</p></td>";
                        //}
                        //else
                        //{
                        //    collegeFaculty += "<td colspan='3'><p align='center'>" + "111-" + strFacultyPhoto.Trim() + "</p></td>";
                        //}
                    }
                    else
                    {
                        collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                    }
                }
                else
                {
                    collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                }
                collegeFaculty += "<td colspan='1'><p align='center'><strong>" + strcheckList + "</strong></p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'><strong>" + Reason1 + "</strong></p></td>";
                collegeFaculty += "</tr>";
                count++;
            }
            collegeFaculty += "</tbody>";
            collegeFaculty += "</table>";
            return collegeFaculty;
        }

        public string LaboratoriesDetails(int collegeId, string contents)
        {
            //  collegeId = 68;
            string strLaboratoriesDetails = string.Empty;
            string Equipmentdate = string.Empty;
            string chalanaDate = string.Empty;

            int sno = 1;

            var labMaster = db.jntuh_lab_master;
            List<Lab> labs = (from lm in db.jntuh_lab_master
                              join l in db.jntuh_appeal_college_laboratories on lm.id equals l.EquipmentID
                              where (l.CollegeID == collegeId)
                              select new Lab
                              {
                                  degree = lm.jntuh_degree.degree,
                                  department = lm.jntuh_department.departmentName,
                                  specializationName = lm.jntuh_specialization.specializationName,
                                  Semester = lm.Semester,
                                  year = lm.Year,
                                  Labcode = lm.Labcode,
                                  LabName = lm.LabName,
                                  AvailableArea = l.AvailableArea,
                                  RoomNumber = l.RoomNumber,
                                  EquipmentName = l.EquipmentName,
                                  Make = l.Make,
                                  Model = l.Model,
                                  EquipmentUniqueID = l.EquipmentUniqueID,
                                  AvailableUnits = l.AvailableUnits,
                                  specializationId = lm.SpecializationID,
                                  // EquipmentPhoto=l.EquipmentPhoto,
                                  ViewEquipmentPhoto = l.EquipmentPhoto,
                                  EquipmentDateOfPurchasing = l.EquipmentDateOfPurchasing,
                                  DelivaryChalanaDate = l.DelivaryChalanaDate
                              }).ToList();
            labs = labs.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList();

            int[] SpecializationIDs = labs.Select(l => l.specializationId).Distinct().ToArray();

            foreach (var speclializationId in SpecializationIDs)
            {
                string strLabName = string.Empty;
                sno = 1;
                string strviewEquimentdata = string.Empty;
                var specializationDetails = db.jntuh_specialization.Where(s => s.id == speclializationId).Select(s => new
                {
                    specialization = s.specializationName,
                    department = s.jntuh_department.departmentName,
                    degree = s.jntuh_department.jntuh_degree.degree
                }).FirstOrDefault();

                strLabName = specializationDetails.degree + "- " + specializationDetails.department + "- " + specializationDetails.specialization;
                //strLaboratoriesDetails += strLabName + "<br />";
                strLaboratoriesDetails += "<strong><u>" + strLabName + "</u></strong> <br /> <br />";

                strLaboratoriesDetails += "<table border='1' cellspacing='0' cellpadding='3'>";
                strLaboratoriesDetails += "<thead>";
                strLaboratoriesDetails += "<tr>";
                strLaboratoriesDetails += "<td  colspan='2'><p align='center'>S.No</p></td>";
                //strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Degree</p></td>";
                //strLaboratoriesDetails += "<td  colspan='4'><p align='left'>Dept.</p></td>";
                //strLaboratoriesDetails += "<td  colspan='7'><p align='left'>Specialization</p></td>";
                if (specializationDetails.degree == "B.Tech" || specializationDetails.degree == "B.Pharmacy")
                {
                    strLaboratoriesDetails += "<td  colspan='2'><p align='left'>Year</p></td>";
                    strLaboratoriesDetails += "<td  colspan='2'><p align='left'>Sem.</p></td>";
                    strLaboratoriesDetails += "<td  colspan='2'><p align='left'>Lab Code</p></td>";
                }
                strLaboratoriesDetails += "<td  colspan='8'><p align='left'>Lab Name</p></td>";
                strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Area (in Sqm)</p></td>";
                strLaboratoriesDetails += "<td  colspan='2'><p align='left'>Room No</p></td>";
                strLaboratoriesDetails += "<td  colspan='8'><p align='left'>Equipment Name</p></td>";
                strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Make</p></td>";
                strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Model</p></td>";
                strLaboratoriesDetails += "<td  colspan='4'><p align='left'>Equipment UniqueID</p></td>";
                strLaboratoriesDetails += "<td  colspan='4'><p align='left'>Equiment Photo</p></td>";
                strLaboratoriesDetails += "<td  colspan='5'><p align='left'>EquipmentDateOfPurchasing</p></td>";
                strLaboratoriesDetails += "<td  colspan='5'><p align='left'>DelivaryChalanaDate</p></td>";
                strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Available Units</p></td>";
                strLaboratoriesDetails += "</tr>";
                strLaboratoriesDetails += "</thead>";
                strLaboratoriesDetails += "<tbody>";
                // serverURL = "http://112.133.193.228:75/";
                foreach (var item in labs.Where(l => l.specializationId == speclializationId).ToList())
                {
                    if (item.ViewEquipmentPhoto != null && item.ViewEquipmentPhoto != "")
                    {
                        strviewEquimentdata = "/Content/Upload/EquipmentsPhotos/" + item.ViewEquipmentPhoto;
                    }
                    else
                    {
                        strviewEquimentdata = "/Content/Images/no-photo.gif";
                    }
                    string path = @"~" + strviewEquimentdata.Replace("%20", " ");
                    // string path = @"" + strviewEquimentdata.Replace("%20", " ");
                    path = System.Web.HttpContext.Current.Server.MapPath(path);

                    if (item.DelivaryChalanaDate != null)
                    {
                        chalanaDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.DelivaryChalanaDate.ToString());
                    }


                    if (item.EquipmentDateOfPurchasing != null)
                    {
                        Equipmentdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.EquipmentDateOfPurchasing.ToString());
                    }


                    strLaboratoriesDetails += "<tr>";
                    strLaboratoriesDetails += "<td  align='center' colspan='2'><p align='center'>" + sno + "</p></td>";
                    //strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.degree + "</td>";
                    //strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.department + "</td>";
                    //strLaboratoriesDetails += "<td  align='left' colspan='7'>" + item.specializationName + "</td>";
                    if (item.degree == "B.Tech" || item.degree == "B.Pharmacy")
                    {
                        strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.year + "</td>";
                        strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Semester + "</td>";
                        strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Labcode + "</td>";
                    }
                    strLaboratoriesDetails += "<td  align='left' colspan='8'>" + item.LabName + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.AvailableArea + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.RoomNumber + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='8'>" + item.EquipmentName + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.Make + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.Model + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.EquipmentUniqueID + "</td>";



                    // strLaboratoriesDetails += "<td  width='100' align='center' colspan='4'><p align='center'><img src='" + serverURL + strviewEquimentdata.Replace("%20", " ") + "' align='center'  width='40' height='50' /></p></td>";

                    if (!string.IsNullOrEmpty(item.ViewEquipmentPhoto))
                    {
                        if (System.IO.File.Exists(path))
                        {

                            strLaboratoriesDetails += "<td  width='100' align='center' colspan='4'><p align='center'><img src='" + HtmlEncoder.Encode(path).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";

                        }
                        else
                        {
                            strLaboratoriesDetails += "<td width='100' align='center' colspan='4'><p align='center'></p></td></tr>";
                        }
                    }
                    else
                    {
                        strLaboratoriesDetails += "<td width='100' align='center' colspan='4'><p align='center'></p></td></tr>";
                    }
                    strLaboratoriesDetails += "<td  align='left' colspan='5'>" + Equipmentdate + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='5'>" + chalanaDate + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.AvailableUnits + "</td>";
                    strLaboratoriesDetails += "</tr>";
                    sno++;
                }
                strLaboratoriesDetails += "</tbody></table>";
            }
            contents = contents.Replace("##LaboratoriesDetails##", strLaboratoriesDetails);
            return contents;
        }

        public string ExistingIntakeDetails(int collegeId, string contents)
        {
            // collegeId = 186;
            string strExistingIntakeDetails = string.Empty;
            int sno = 0;
            int totalApprovedIntake1 = 0;
            int totalApprovedIntake2 = 0;
            int totalApprovedIntake3 = 0;
            int totalApprovedIntake4 = 0;
            int totalApprovedIntake5 = 0;
            int totalAdmittedIntake1 = 0;
            int totalAdmittedIntake2 = 0;
            int totalAdmittedIntake3 = 0;
            int totalAdmittedIntake4 = 0;
            int totalAdmittedIntake5 = 0;
            int totalApproved = 0;
            int totalAdmited = 0;
            int totalPAYApproved = 0;
            int totalPAYAdmited = 0;
            int totalPAYProposed = 0;

            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            string FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            string SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            string ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            string FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            string FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            int PAY = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();



            List<jntuh_appeal_college_intake_existing> intake = db.jntuh_appeal_college_intake_existing.Where(i => i.collegeId == collegeId).ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            var jntuh_specialization = db.jntuh_specialization;
            var jntuh_department = db.jntuh_department;
            var jntuh_degree = db.jntuh_degree;
            var jntuh_shift = db.jntuh_shift;
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
                newIntake.Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.DepartmentID = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.Department = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.FirstOrDefault()).ToList();
            collegeIntakeExisting = collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            if (collegeIntakeExisting.Count() != 0)
            {
                strExistingIntakeDetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 8px;'>";
                strExistingIntakeDetails += "<tbody>";
                strExistingIntakeDetails += "<tr>";
                strExistingIntakeDetails += "<td width='28' rowspan='3' colspan='1'><p align='center'>S.No</p></td>";
                strExistingIntakeDetails += "<td width='56' rowspan='3' colspan='3'><p align='left'>Degree</p><p align='left'>*</p></td>";
                strExistingIntakeDetails += "<td width='63' rowspan='3' colspan='4'><p align='left'>Department</p><p align='left'>**</p></td>";
                strExistingIntakeDetails += "<td width='200' rowspan='3' colspan='4'><p align='left'>Specialization</p><p align='left'>***</p></td>";
                // strExistingIntakeDetails += "<td width='42' rowspan='3' colspan='1' style='font-size: 9px; line-height: 10px;'><p align='center'>Shift</p><p align='center'>#</p></td>";
                strExistingIntakeDetails += "<td width='500' colspan='10'><p align='center'>Sanctioned & Actual Admitted Intake as per Academic Year</p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>PI</p><p align='left'></p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>CS</p><p align='left'></p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center'>NBA accreditation Period (if exists)</p></td></tr>";
                strExistingIntakeDetails += "<tr><td width='100' colspan='2'><p align='center'>" + FifthYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + FourthYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + ThirdYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + SecondYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + FirstYear + "</p></td>";


                strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center' style='font-style: 7px;'>(DD/MM/YYY)</p></td>";
                strExistingIntakeDetails += "</tr>";
                strExistingIntakeDetails += "<tr style='font-size: 7px;'>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top'><p align='center'>From</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top'><p align='center'>To</p></td>";
                strExistingIntakeDetails += "</tr>";



                foreach (var item in collegeIntakeExisting)
                {
                    sno++;

                    if (item.nbaFrom != null)
                        item.nbaFromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                    if (item.nbaTo != null)
                        item.nbaToDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());
                    item.ProposedIntake = intake.Where(i => i.collegeId == collegeId && i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.proposedIntake).FirstOrDefault();
                    if (item.ProposedIntake != null)
                        totalPAYProposed += (int)item.ProposedIntake;
                    item.courseStatus = intake.Where(i => i.collegeId == collegeId && i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.courseStatus).FirstOrDefault();
                    //  if (item.ApprovedIntake != null)
                    //    totalPAYApproved += (int)item.ApprovedIntake;
                    //totalPAYAdmited += item.admittedIntake;

                    if (item.courseStatus == "Closure")
                    {
                        item.courseStatus = "C";
                    }
                    else if (item.courseStatus == "New")
                    {
                        item.courseStatus = "N";
                    }
                    else if (item.courseStatus == "Increase")
                    {
                        item.courseStatus = "I";
                    }
                    else if (item.courseStatus == "Nochange")
                    {
                        item.courseStatus = "NC";
                    }
                    else if (item.courseStatus == "Decrease")
                    {
                        item.courseStatus = "D";
                    }
                    else
                    {
                        item.courseStatus = "";
                    }





                    item.approvedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 1);
                    item.admittedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake1 += item.approvedIntake1;
                    totalAdmittedIntake1 += item.admittedIntake1;

                    item.approvedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 1);
                    item.admittedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake2 += item.approvedIntake2;
                    totalAdmittedIntake2 += item.admittedIntake2;

                    item.approvedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 1);
                    item.admittedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake3 += item.approvedIntake3;
                    totalAdmittedIntake3 += item.admittedIntake3;

                    item.approvedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 1);
                    item.admittedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake4 += item.approvedIntake4;
                    totalAdmittedIntake4 += item.admittedIntake4;

                    item.approvedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 1);
                    item.admittedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake5 += item.approvedIntake5;
                    totalAdmittedIntake5 += item.admittedIntake5;

                    strExistingIntakeDetails += "<tr>";
                    strExistingIntakeDetails += "<td colspan='1' width='28'><p align='center'>" + sno + "</p></td>";
                    strExistingIntakeDetails += "<td colspan='3' width='56'>" + item.Degree + "</td>";
                    strExistingIntakeDetails += "<td colspan='4' width='63'>" + item.Department + "</td>";
                    strExistingIntakeDetails += "<td colspan='4' width='200'>" + item.Specialization + "</td>";
                    //  strExistingIntakeDetails += "<td colspan='1' width='42'>" + item.Shift + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake5.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake5.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake4.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake4.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake3.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake3.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake2.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake2.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='42'>" + item.approvedIntake1.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake1.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1'>" + item.ProposedIntake + "</td>";
                    strExistingIntakeDetails += "<td colspan='1'>" + item.courseStatus + "</td>";
                    strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaFromDate + "</td>";
                    strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaToDate + "</td>";
                    strExistingIntakeDetails += "</tr>";
                    if (item.Degree == "Pharm.D PB")//6
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5 + item.admittedIntake6;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5 + item.approvedIntake6;
                    }
                    else if (item.Degree == "MAM" || item.Degree == "MTM" || item.Degree == "Pharm.D")//5
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5;
                    }
                    else if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")//4
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4;
                    }
                    else if (item.Degree == "MCA")//3
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA") //2
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2;
                    }
                }
                // totalAdmited += totalAdmittedIntake1 + totalAdmittedIntake2 + totalAdmittedIntake3 + totalAdmittedIntake4 + totalAdmittedIntake5 + totalPAYAdmited;
                // totalApproved += totalApprovedIntake1 + totalApprovedIntake2 + totalApprovedIntake3 + totalApprovedIntake4 + totalApprovedIntake5 + totalPAYApproved;

                strExistingIntakeDetails += "<tr><td width='337' colspan='12'><p align='right'>Total =</p></td><td width='50' colspan='1' align='center'>" + totalApprovedIntake5 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake5 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake4 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake4 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake3 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake3 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake2 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake2 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake1 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake1 + "</td><td width='50' colspan='1' align='center'>" + totalPAYApproved + "</td><td width='50' colspan='1' align='center'>" + totalPAYProposed + "</td><td width='50' colspan='2' valign='top' align='center'></td><td width='50' colspan='2' valign='top' align='center'></td></tr>";
                strExistingIntakeDetails += "<tr><td colspan='12'><p align='right'>Total Admitted / Total Sanctioned =</p></td><td colspan='16' width='600'>" + totalAdmited + '/' + totalApproved + "</td></tr>";
                strExistingIntakeDetails += "</tbody></table>";
                contents = contents.Replace("##ExistingIntakeDetails##", strExistingIntakeDetails);
            }
            else
            {
                contents = contents.Replace("##ExistingIntakeDetails##", "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 8px;'><tr><td style='text-align:center'>No Data Available</td></tr></table>");
            }

            return contents;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            if (flag == 1)
                intake = db.jntuh_appeal_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
            else
                intake = db.jntuh_appeal_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            return intake;
        }

        public string Remarks(int collegeId, string contents)
        {
            // collegeId = 375;
            string remarkstabledata = string.Empty;
            string remarks = db.jntuh_appeal_college_edit_status.Where(e => e.collegeId == collegeId).Select(e => e.Remarks).FirstOrDefault();
            remarkstabledata += "<table border='1' width='100%' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            remarkstabledata += "<tbody>";
            remarkstabledata += "<tr>";
            remarkstabledata += "<td width='10%'><span>Remarks</span></td>";
            remarkstabledata += "<td width='4%'style='text-align:center' ><span>:</span></td>";
            remarkstabledata += "<td width='86%'>" + remarks + "</td>";
            remarkstabledata += "</tr>";
            remarkstabledata += "</tbody>";
            remarkstabledata += "</table>";
            contents = contents.Replace("##REMARKS##", remarkstabledata);
            return contents;
        }



        #endregion

        #region For Faculty Add/Reactivate Logic

        //Appeal Faculty Details
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult ViewFacultyDetails(int? collegeId)
        {
            var colgids = db.jntuh_appeal_college_edit_status.Where(i => i.IsCollegeEditable == false).GroupBy(e => e.collegeId).Select(e => e.Key).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive && colgids.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            var appealfacultyDetails = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
            var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            //var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            var facultyDetails = new List<CollegeFaculty>();
            if (collegeId != null)
            {
                appealfacultyDetails = appealfacultyDetails.Where(e => e.collegeId == collegeId).ToList();
                foreach (var item in appealfacultyDetails)
                {
                    var faculty = new CollegeFaculty
                    {
                        FacultyRegistrationNumber = item.RegistrationNumber,
                        collegeId = item.collegeId,
                        id = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.id).FirstOrDefault(),
                        facultyFirstName = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.FirstName).FirstOrDefault(),
                        facultyLastName = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.LastName).FirstOrDefault(),
                        facultySurname = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.MiddleName).FirstOrDefault(),
                        ViewNotificationDocument = item.NOtificationReport,
                        ViewAppointmentOrderDocument = item.AppointMentOrder,
                        ViewJoiningReportDocument = item.JoiningOrder,
                        ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes,
                        ViewAppealReverificationSupportDoc = item.AppealReverificationSupportingDocument,
                        isActive = item.isActive
                    };
                    facultyDetails.Add(faculty);
                }
            }
            return View(facultyDetails);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyAddingToCollege(string fid, int? collegeid)
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = 0;
            var appealid = 0;
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            var jntuhAppealFacultyRegistered = db.jntuh_appeal_faculty_registered.AsNoTracking().FirstOrDefault(i => i.existingFacultyId == fId);
            if (jntuhAppealFacultyRegistered != null)
            {
                appealid = jntuhAppealFacultyRegistered.id;
            }
            var facultydetails = db.jntuh_appeal_faculty_registered.Find(appealid);
            var collegefacultyreg = db.jntuh_college_faculty_registered.AsNoTracking().Where(i => i.RegistrationNumber.Trim() == facultyregistered.RegistrationNumber.Trim()).ToList();
            var collegeFaculty = new jntuh_college_faculty_registered();
            if (facultydetails != null && collegefacultyreg.Count == 0)
            {
                collegeFaculty.collegeId = facultydetails.collegeId;
                collegeFaculty.RegistrationNumber = facultydetails.RegistrationNumber;
                collegeFaculty.IdentifiedFor = facultydetails.IdentifiedFor;
                collegeFaculty.SpecializationId = facultydetails.SpecializationId;
                collegeFaculty.existingFacultyId = facultydetails.existingFacultyId;
                collegeFaculty.createdBy = userId;
                collegeFaculty.createdOn = DateTime.Now;
                db.jntuh_college_faculty_registered.Add(collegeFaculty);
                db.SaveChanges();
                TempData["Success"] = "Registration Number Is Successfully Added to College Faculty..";
                facultydetails.isActive = true;
                db.SaveChanges();
            }
            else if (facultydetails != null && collegefacultyreg.Count > 0)
            {
                var id = 0;
                var createdby = 0;
                DateTime? createdon = null;
                var jntuhCollegeFacultyRegistered = collegefacultyreg.FirstOrDefault();
                if (jntuhCollegeFacultyRegistered != null)
                {
                    id = jntuhCollegeFacultyRegistered.id;
                    if (jntuhCollegeFacultyRegistered.createdBy != null)
                        createdby = (int)jntuhCollegeFacultyRegistered.createdBy;
                    createdon = jntuhCollegeFacultyRegistered.createdOn;
                }
                collegeFaculty.id = id;
                collegeFaculty.collegeId = facultydetails.collegeId;
                collegeFaculty.RegistrationNumber = facultydetails.RegistrationNumber;
                collegeFaculty.IdentifiedFor = facultydetails.IdentifiedFor;
                collegeFaculty.existingFacultyId = facultydetails.existingFacultyId;
                collegeFaculty.SpecializationId = facultydetails.SpecializationId;
                collegeFaculty.createdBy = createdby;
                collegeFaculty.createdOn = createdon;
                collegeFaculty.updatedBy = userId;
                collegeFaculty.updatedOn = DateTime.Now;
                db.Entry(collegeFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Registration Number Is Successfully Updated to College Faculty..";
                facultydetails.isActive = true;
                db.SaveChanges();
            }
            else if (facultydetails == null)
            {
                TempData["Error"] = "Registration Number Was not found";
            }
            return RedirectToAction("ViewFacultyDetails", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyReactivateAllFlags(string fid, int? collegeid)
        {
            var fId = 0;
            var id = 0;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            var jntuhAppealFacultyRegistered = db.jntuh_appeal_faculty_registered.AsNoTracking().FirstOrDefault(i => i.existingFacultyId == fId);
            if (jntuhAppealFacultyRegistered != null)
            {
                id = jntuhAppealFacultyRegistered.id;
            }
            var facultydetails = db.jntuh_appeal_faculty_registered.Find(id);
            if (facultyregistered == null)
            {
                TempData["Error"] = "RegistrationNumber Not Found.";
                return RedirectToAction("ViewFacultyDetails", new { collegeid = collegeid });
            }

            facultyregistered.Absent = false;
            facultyregistered.InvalidPANNumber = false;
            facultyregistered.NoSCM = false;
            facultyregistered.NoForm16 = false;
            facultyregistered.NotQualifiedAsperAICTE = false;
            facultyregistered.MultipleRegInSameCollege = false;
            facultyregistered.MultipleRegInDiffCollege = false;
            facultyregistered.SamePANUsedByMultipleFaculty = false;
            facultyregistered.PhotoCopyofPAN = false;
            facultyregistered.AppliedPAN = false;
            facultyregistered.LostPAN = false;
            facultyregistered.OriginalsVerifiedUG = false;
            facultyregistered.OriginalsVerifiedPG = false;
            facultyregistered.OriginalsVerifiedPHD = false;
            facultyregistered.FacultyVerificationStatus = false;
            facultyregistered.IncompleteCertificates = false;
            facultyregistered.FalsePAN = false;
            facultyregistered.PHDundertakingnotsubmitted = false;
            facultyregistered.Blacklistfaculy = false;
            facultyregistered.updatedBy = userId;
            facultyregistered.updatedOn = DateTime.Now;
            db.SaveChanges();
            TempData["Success"] = "Faculty Registration Number ( " + facultyregistered.RegistrationNumber + " ) Successfully Re-activated..";

            if (facultydetails != null)
            {
                facultydetails.isActive = true;
                db.SaveChanges();
            }

            return RedirectToAction("ViewFacultyDetails", new { collegeid = collegeid });
        }

        #endregion




    }
}
