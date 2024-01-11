using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class ValidateCollegeDataController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /ValidateCollegeData/ValidateCollegeDataIndex
        [Authorize(Roles = "DataEntry,Admin,College")]
        public ActionResult ValidateCollegeDataIndex(string id)
        {
            int collegeId = 0;

            //if collegeId is not zero for corresponding table it return submitted
            string submitted = "Submitted";

            //if collegeId is zero for corresponding table it return not submitted
            string notSubmitted = "Not Submitted";

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                collegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            else
            {
                collegeId = userCollegeID;
            }

            #region College Information

            int collegeInformationCollegeId = db.jntuh_college.Where(college => college.id == collegeId)
                                                              .Select(college => college.id)
                                                              .FirstOrDefault();
            if (collegeInformationCollegeId != 0)
            {
                ViewBag.CollegeInformationCollegeDetails = submitted;
            }
            else
            {
                ViewBag.CollegeInformationCollegeDetails = notSubmitted;
            }

            int collegeAddressId = db.jntuh_address.Where(address => address.collegeId == collegeId && address.addressTye == "COLLEGE")
                                                   .Select(address => address.id)
                                                   .FirstOrDefault();
            if (collegeAddressId != 0)
            {
                ViewBag.CollegeInformationCollegeAddressDetails = submitted;
            }
            else
            {
                ViewBag.CollegeInformationCollegeAddressDetails = notSubmitted;
            }

            var collegeDegreeId = db.jntuh_college_degree.Where(degree => degree.isActive == true && degree.collegeId == collegeId)
                                                         .ToList();
            if (collegeDegreeId.Count() != 0)
            {
                ViewBag.CollegeInformationCollegeDegreeDetails = submitted;
            }
            else
            {
                ViewBag.CollegeInformationCollegeDegreeDetails = notSubmitted;
            }

            var collegeAffiliation = db.jntuh_college_affiliation.Where(affiliation => affiliation.collegeId == collegeId)
                                                                 .ToList();
            if (collegeAffiliation.Count() != 0)
            {
                ViewBag.CollegeInformationCollegeAffiliationDetails = submitted;
            }
            else
            {
                ViewBag.CollegeInformationCollegeAffiliationDetails = notSubmitted;
            }

            #endregion

            #region Educational Society

            int estlablishmentId = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == collegeId)
                                                                 .Select(establishment => establishment.id)
                                                                 .FirstOrDefault();
            if (estlablishmentId != 0)
            {
                ViewBag.EstablishMentDetails = submitted;
            }
            else
            {
                ViewBag.EstablishMentDetails = notSubmitted;
            }
            int societyAddressId = db.jntuh_address.Where(address => address.collegeId == collegeId && address.addressTye == "SOCIETY")
                                                   .Select(address => address.id)
                                                   .FirstOrDefault();
            if (societyAddressId != 0)
            {
                ViewBag.SocietyAddressDetails = submitted;
            }
            else
            {
                ViewBag.SocietyAddressDetails = notSubmitted;
            }

            #endregion

            #region Principal / Director

            int existrincipalId = db.jntuh_college_principal_director.Where(principal => principal.collegeId == collegeId &&
                                                                                                    principal.type.Equals("PRINCIPAL"))
                                                                        .Select(principal => principal.id)
                                                                        .FirstOrDefault();

            int existDirectorId = db.jntuh_college_principal_director.Where(director => director.collegeId == collegeId &&
                                                                                                    director.type.Equals("DIRECTOR"))
                                                                       .Select(director => director.id)
                                                                       .FirstOrDefault();
            if (existrincipalId != 0)
            {
                ViewBag.PrincipalDetails = submitted;
            }
            else
            {
                ViewBag.PrincipalDetails = notSubmitted;
            }
            if (existDirectorId != 0)
            {
                ViewBag.DirectorDetails = submitted;
            }
            else
            {
                ViewBag.DirectorDetails = notSubmitted;
            }
            #endregion

            #region Chairperson

            int existChairpersonId = db.jntuh_college_chairperson.Where(committeechairperson => committeechairperson.collegeId == collegeId)
                                                   .Select(committeechairperson => committeechairperson.id)
                                                   .FirstOrDefault();

            int existchairPersonAddressId = db.jntuh_address.Where(committeeAddress => committeeAddress.collegeId == collegeId && committeeAddress.addressTye == "SECRETARY")
                                                           .Select(committeeAddress => committeeAddress.id)
                                                           .FirstOrDefault();
            if (existChairpersonId != 0)
            {
                ViewBag.ChairpersonDetails = submitted;
            }
            else
            {
                ViewBag.ChairpersonDetails = notSubmitted;
            }
            if (existchairPersonAddressId != 0)
            {
                ViewBag.ChairpersonAddressDetails = submitted;
            }
            else
            {
                ViewBag.ChairpersonAddressDetails = notSubmitted;
            }
            #endregion

            #region Other Colleges & Other Courses

            int existOtherCollegesId = db.jntuh_society_other_colleges.Where(otherCollege => otherCollege.collegeId == collegeId)
                                                                                .Select(otherCollege => otherCollege.id)
                                                                                .FirstOrDefault();
            int existOtherCoursesId = db.jntuh_college_other_university_courses.Where(otherCourses => otherCourses.collegeId == collegeId)
                                                                                 .Select(otherCourses => otherCourses.id)
                                                                                 .FirstOrDefault();
            if (existOtherCollegesId != 0)
            {
                ViewBag.OtherColleges = submitted;
            }
            else
            {
                ViewBag.OtherColleges = notSubmitted;
            }
            if (existOtherCoursesId != 0)
            {
                ViewBag.OtherCourses = submitted;
            }
            else
            {
                ViewBag.OtherCourses = notSubmitted;
            }
            #endregion

            #region Land Information

            int existLandId = db.jntuh_college_land.Where(land => land.collegeId == collegeId)
                                                     .Select(land => land.id)
                                                     .FirstOrDefault();
            int existlandregistrationId = db.jntuh_college_land_registration.Where(landRegistration => landRegistration.collegeId == collegeId)
                                                                              .Select(landRegistration => landRegistration.id)
                                                                              .FirstOrDefault();
            if (existLandId != 0)
            {
                ViewBag.LandInformation = submitted;
            }
            else
            {
                ViewBag.LandInformation = notSubmitted;
            }
            if (existlandregistrationId != 0)
            {
                ViewBag.LandRegistrationInformation = submitted;
            }
            else
            {
                ViewBag.LandRegistrationInformation = notSubmitted;
            }
            
            #endregion

            #region Administrative Land

            int[] requirementArea = db.jntuh_area_requirement.Where(requirement => requirement.isActive == true && requirement.areaType == "ADMINISTRATIVE")
                                                  .Select(requirement => requirement.id)
                                                  .ToArray();

            int existAdminlandId = db.jntuh_college_area.Where(area => area.collegeId == collegeId && requirementArea.Contains(area.areaRequirementId))
                                                          .Select(area => area.id)
                                                          .FirstOrDefault();
            if (existAdminlandId != 0)
            {
                ViewBag.AdministrativeLand = submitted;
            }
            else
            {
                ViewBag.AdministrativeLand = notSubmitted;
            }

            #endregion

            #region Instructional Land

            int[] requirementInstructionalArea = db.jntuh_area_requirement.Where(requirement => requirement.isActive == true && requirement.areaType == "INSTRUCTIONAL")
                                                  .Select(requirement => requirement.id)
                                                  .ToArray();

            int existInstructionalId = db.jntuh_college_area.Where(area => area.collegeId == collegeId && requirementInstructionalArea.Contains(area.areaRequirementId))
                                                          .Select(area => area.id)
                                                          .FirstOrDefault();
            if (existInstructionalId != 0)
            {
                ViewBag.InstructionalLand = submitted;
            }
            else
            {
                ViewBag.InstructionalLand = notSubmitted;
            }

            #endregion

            #region Existing Intake

            int existCollegeExistIntakeId = db.jntuh_college_intake_existing.Where(existingIntake => existingIntake.collegeId == collegeId)
                                                                              .Select(existingIntake => existingIntake.id)
                                                                              .FirstOrDefault();
            if (existCollegeExistIntakeId != 0)
            {
                ViewBag.ExistingIntake = submitted;
            }
            else
            {
                ViewBag.ExistingIntake = notSubmitted;
            }
            #endregion

            #region Proposed Intake

            int existCollegeProposedIntakeId = db.jntuh_college_intake_proposed.Where(proposedIntake => proposedIntake.collegeId == collegeId)
                                                                                 .Select(proposedIntake => proposedIntake.id)
                                                                                 .FirstOrDefault();
            if (existCollegeProposedIntakeId != 0)
            {
                ViewBag.ProposedIntake = submitted;
            }
            else
            {
                ViewBag.ProposedIntake = notSubmitted;
            }

            #endregion

            #region Academic Performance

            int existAcademicPerformanceId = db.jntuh_college_academic_performance.Where(academicPerformance => academicPerformance.collegeId == collegeId)
                                                                                    .Select(academicPerformance => academicPerformance.id)
                                                                                    .FirstOrDefault();
            if (existAcademicPerformanceId != 0)
            {
                ViewBag.AcademicPerformance = submitted;
            }
            else
            {
                ViewBag.AcademicPerformance = notSubmitted;
            }

            #endregion

            #region Teaching Faculty

            int teachingFacultyTypeId = db.jntuh_faculty_type.Where(facultyType => facultyType.facultyType == "Teaching")
                                                     .Select(facultyType => facultyType.id)
                                                     .FirstOrDefault();
            int[] teachingFacultyId = db.jntuh_college_faculty.Where(faculty => faculty.collegeId == collegeId &&
                                                                                  faculty.facultyTypeId == teachingFacultyTypeId)
                                                                .Select(faculty => faculty.id)
                                                                .ToArray();
            List<jntuh_faculty_education> teachingFacultyEducationDetails = db.jntuh_faculty_education.Where(education => teachingFacultyId.Contains(education.facultyId))
                                                                                                      .ToList();
            List<jntuh_faculty_subjects> teachingFacultySubjectDetails = db.jntuh_faculty_subjects.Where(subject => teachingFacultyId.Contains(subject.facultyId))
                                                                                                  .ToList();
            if (teachingFacultyId.Count() != 0)
            {
                ViewBag.TeachingFacultyDetails = submitted;
            }
            else
            {
                ViewBag.TeachingFacultyDetails = notSubmitted;
            }
            if (teachingFacultyEducationDetails.Count() != 0)
            {
                ViewBag.TeachingFacultyEucationalQualification = submitted;
            }
            else
            {
                ViewBag.TeachingFacultyEucationalQualification = notSubmitted;
            }
            if (teachingFacultySubjectDetails.Count() != 0)
            {
                ViewBag.TeachingFacultySubjects = submitted;
            }
            else
            {
                ViewBag.TeachingFacultySubjects = notSubmitted;
            }

            #endregion

            #region Non-Teaching Staff

            int nonTeachingFacultyTypeId = db.jntuh_faculty_type.Where(facultyType => facultyType.facultyType == "Non-Teaching")
                                                     .Select(facultyType => facultyType.id)
                                                     .FirstOrDefault();
            int[] nonTeachingFacultyId = db.jntuh_college_faculty.Where(faculty => faculty.collegeId == collegeId &&
                                                                                  faculty.facultyTypeId == nonTeachingFacultyTypeId)
                                                                .Select(faculty => faculty.id)
                                                                .ToArray();
            List<jntuh_faculty_education> nonTeachingFacultyEducationDetails = db.jntuh_faculty_education.Where(education => nonTeachingFacultyId.Contains(education.facultyId))
                                                                                                      .ToList();
            List<jntuh_faculty_subjects> nonTeachingFacultySubjectDetails = db.jntuh_faculty_subjects.Where(subject => nonTeachingFacultyId.Contains(subject.facultyId))
                                                                                                  .ToList();
            if (nonTeachingFacultyId.Count() != 0)
            {
                ViewBag.NonTeachingFacultyDetails = submitted;
            }
            else
            {
                ViewBag.NonTeachingFacultyDetails = notSubmitted;
            }
            if (nonTeachingFacultyEducationDetails.Count() != 0)
            {
                ViewBag.NonTeachingFacultyEucationalQualification = submitted;
            }
            else
            {
                ViewBag.NonTeachingFacultyEucationalQualification = notSubmitted;
            }
            if (nonTeachingFacultySubjectDetails.Count() != 0)
            {
                ViewBag.NonTeachingFacultySubjects = submitted;
            }
            else
            {
                ViewBag.NonTeachingFacultySubjects = notSubmitted;
            }

            #endregion

            #region Technical Staff

            int technicalFacultyTypeId = db.jntuh_faculty_type.Where(facultyType => facultyType.facultyType == "Technical")
                                                     .Select(facultyType => facultyType.id)
                                                     .FirstOrDefault();
            int[] technicalFacultyId = db.jntuh_college_faculty.Where(faculty => faculty.collegeId == collegeId &&
                                                                                  faculty.facultyTypeId == technicalFacultyTypeId)
                                                                .Select(faculty => faculty.id)
                                                                .ToArray();
            List<jntuh_faculty_education> technicalFacultyEducationDetails = db.jntuh_faculty_education.Where(education => technicalFacultyId.Contains(education.facultyId))
                                                                                                      .ToList();
            List<jntuh_faculty_subjects> technicalFacultySubjectDetails = db.jntuh_faculty_subjects.Where(subject => technicalFacultyId.Contains(subject.facultyId))
                                                                                                  .ToList();
            if (technicalFacultyId.Count() != 0)
            {
                ViewBag.TechnicalFacultyDetails = submitted;
            }
            else
            {
                ViewBag.TechnicalFacultyDetails = notSubmitted;
            }
            if (technicalFacultyEducationDetails.Count() != 0)
            {
                ViewBag.TechnicalFacultyEucationalQualification = submitted;
            }
            else
            {
                ViewBag.TechnicalFacultyEucationalQualification = notSubmitted;
            }
            if (technicalFacultySubjectDetails.Count() != 0)
            {
                ViewBag.TechnicalFacultySubjects = submitted;
            }
            else
            {
                ViewBag.TechnicalFacultySubjects = notSubmitted;
            }

            #endregion

            #region Labs

            int existCollegeLabId = db.jntuh_college_lab.Where(lab => lab.collegeId == collegeId)
                                                          .Select(lab => lab.id)
                                                          .FirstOrDefault();
            if (existCollegeLabId != 0)
            {
                ViewBag.Labs = submitted;
            }
            else
            {
                ViewBag.Labs = notSubmitted;
            }

            #endregion

            #region Library Information

            int existCollegeLibraryId = db.jntuh_college_library.Where(library => library.collegeId == collegeId)
                                                                  .Select(library => library.id)
                                                                  .FirstOrDefault();
            if (existCollegeLibraryId != 0)
            {
                ViewBag.LibraryInformation = submitted;
            }
            else
            {
                ViewBag.LibraryInformation = notSubmitted;
            }
            #endregion

            #region Library Books (Course Wise)

            int existLibraryDetailsId = db.jntuh_college_library_details.Where(l => l.collegeId == collegeId)
                                                    .Select(l => l.id).FirstOrDefault();

            if (existLibraryDetailsId != 0)
            {
                ViewBag.LibraryBooks = submitted;
            }
            else
            {
                ViewBag.LibraryBooks = notSubmitted;
            }
            #endregion

            #region Computers - Details

            int existComputerLabId = db.jntuh_college_computer_lab.Where(computerLab => computerLab.collegeId == collegeId)
                                                                    .Select(computerLab => computerLab.id)
                                                                    .FirstOrDefault();
            if (existComputerLabId != 0)
            {
                ViewBag.ComputerDetails = submitted;
            }
            else
            {
                ViewBag.ComputerDetails = notSubmitted;
            }
            #endregion

            #region Computers - Students Ratio

            int computerStudentRatioId = db.jntuh_college_computer_student_ratio.Where(computerStudentRatio => computerStudentRatio.collegeId == collegeId)
                                                                              .Select(computerStudentRatio => computerStudentRatio.id)
                                                                              .FirstOrDefault();

            if (computerStudentRatioId != 0)
            {
                ViewBag.ComputerStudentsRatio = submitted;
            }
            else
            {
                ViewBag.ComputerStudentsRatio = notSubmitted;
            }

            #endregion

            #region Internet Bandwidth

            int interNetBandwidthId = db.jntuh_college_internet_bandwidth.Where(interNetBandWidth => interNetBandWidth.collegeId == collegeId)
                                                                         .Select(interNetBandWidth => interNetBandWidth.id)
                                                                         .FirstOrDefault();
            if (interNetBandwidthId != 0)
            {
                ViewBag.InternetBandwidth = submitted;
            }
            else
            {
                ViewBag.InternetBandwidth = notSubmitted;
            }

            #endregion

            #region Legal Software

            int legalSoftWareId = db.jntuh_college_legal_software.Where(softWare => softWare.collegeId == collegeId)
                                                                 .Select(softWare => softWare.id)
                                                                 .FirstOrDefault();
            if (legalSoftWareId != 0)
            {
                ViewBag.LegalSoftware = submitted;
            }
            else
            {
                ViewBag.LegalSoftware = notSubmitted;
            }

            #endregion

            #region Printers

            int printersId = db.jntuh_college_computer_lab_printers.Where(printers => printers.collegeId == collegeId)
                                                                   .Select(printers => printers.id)
                                                                   .FirstOrDefault();
            if (printersId != 0)
            {
                ViewBag.Printers = submitted;
            }
            else
            {
                ViewBag.Printers = notSubmitted;
            }
            #endregion

            #region Examination Branch

            int examinationBranchId = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.collegeId == collegeId)
                                                                         .Select(examinationBranch => examinationBranch.id)
                                                                         .FirstOrDefault();

            int examinationBranchStafId = db.jntuh_college_examination_branch_staff.Where(staff => staff.collegeId == collegeId)
                                                                                   .Select(staff => staff.id)
                                                                                   .FirstOrDefault();
            if (examinationBranchId != 0)
            {
                ViewBag.ExaminationBranch = submitted;
            }
            else
            {
                ViewBag.ExaminationBranch = notSubmitted;
            }
            if (examinationBranchStafId != 0)
            {
                ViewBag.ExaminationBranchStaff = submitted;
            }
            else
            {
                ViewBag.ExaminationBranchStaff = notSubmitted;
            }

            #endregion

            #region EDEP Equipment

            int EDEPEquipmentId = db.jntuh_college_examination_branch_edep.Where(equipment => equipment.collegeId == collegeId)
                                                                          .Select(equipment => equipment.id)
                                                                          .FirstOrDefault();
            if (EDEPEquipmentId != 0)
            {
                ViewBag.EDEPEquipment = submitted;
            }
            else
            {
                ViewBag.EDEPEquipment = notSubmitted;
            }

            #endregion

            #region Fee Reimbursement

            //int feeReimbersementId = db.jntuh_college_fee_reimbursement.Where(feeReimbersement => feeReimbersement.collegeId == collegeId)
            //                                                           .Select(feeReimbersement => feeReimbersement.id)
            //                                                           .FirstOrDefault();
            //if (feeReimbersementId != 0)
            //{
            //    ViewBag.FeeReimbursement = submitted;
            //}
            //else
            //{
            //    ViewBag.FeeReimbursement = notSubmitted;
            //}

            #endregion

            #region Grievance Redressal

            int grievanceRedressalCommitteeId = db.jntuh_college_grievance_committee.Where(committee => committee.collegeId == collegeId)
                                                                                    .Select(committee => committee.id)
                                                                                    .FirstOrDefault();
            int grievanceRedressalComplaintsId = db.jntuh_college_grievance_complaints.Where(complaints => complaints.collegeId == collegeId)
                                                                                      .Select(complaints => complaints.id)
                                                                                      .FirstOrDefault();
            if (grievanceRedressalCommitteeId != 0)
            {
                ViewBag.GrievanceRedressalCommittee = submitted;
            }
            else
            {
                ViewBag.GrievanceRedressalCommittee = notSubmitted;
            }
            if (grievanceRedressalComplaintsId != 0)
            {
                ViewBag.GrievanceRedressalComplaints = submitted;
            }
            else
            {
                ViewBag.GrievanceRedressalComplaints = notSubmitted;
            }

            #endregion

            #region Anti-Ragging

            int antiRaggingCommitteeId = db.jntuh_college_antiragging_committee.Where(committee => committee.collegeId == collegeId)
                                                                             .Select(commitee => commitee.id)
                                                                             .FirstOrDefault();
            int antiRaggingComplaintsId = db.jntuh_college_antiragging_complaints.Where(complaints => complaints.collegeId == collegeId)
                                                                                 .Select(complaints => complaints.id)
                                                                                 .FirstOrDefault();

            if (antiRaggingCommitteeId != 0)
            {
                ViewBag.AntiRaggingCommittee = submitted;
            }
            else
            {
                ViewBag.AntiRaggingCommittee = notSubmitted;
            }
            if (antiRaggingComplaintsId != 0)
            {
                ViewBag.AntiRaggingComplaints = submitted;
            }
            else
            {
                ViewBag.AntiRaggingComplaints = notSubmitted;
            }
            #endregion

            #region Sports & Games

            int indoorGamesId = db.jntuh_college_sports.Where(sports => sports.sportsTypeId == 1 && sports.collegeId == collegeId)
                                                       .Select(sports => sports.id)
                                                       .FirstOrDefault();
            int outDoorGamesId = db.jntuh_college_sports.Where(sports => sports.sportsTypeId == 2 && sports.collegeId == collegeId)
                                                       .Select(sports => sports.id)
                                                       .FirstOrDefault();
            int otherFacilitiesId = db.jntuh_college_desirable_others.Where(other => other.collegeId == collegeId)
                                                                     .Select(other => other.id)
                                                                     .FirstOrDefault();
            if (indoorGamesId != 0)
            {
                ViewBag.IndoorGames = submitted;
            }
            else
            {
                ViewBag.IndoorGames = notSubmitted;
            }
            if (outDoorGamesId != 0)
            {
                ViewBag.OutDoorGames = submitted;
            }
            else
            {
                ViewBag.OutDoorGames = notSubmitted;
            }
            if (otherFacilitiesId != 0)
            {
                ViewBag.otherFacilities = submitted;
            }
            else
            {
                ViewBag.otherFacilities = notSubmitted;
            }

            #endregion

            #region Desirable Requirements

            int[] otherDesirableTypeId = db.jntuh_desirable_requirement_type.Where(l => l.isActive == true && l.isHostelRequirement == false)
                                                                            .Select(l => l.id)
                                                                            .ToArray();
            int otherDetailsId = db.jntuh_college_desirable_requirement.Where(a => a.collegeId == userCollegeID && 
                                                                                  otherDesirableTypeId.Contains(a.requirementTypeID))
                                                                       .Select(a => a.id)
                                                                       .FirstOrDefault();

            if (otherDetailsId != 0)
            {
                ViewBag.DesirableRequirements = submitted;
            }
            else
            {
                ViewBag.DesirableRequirements = notSubmitted;
            }

            #endregion

            #region Campus Hostel

            int[] campusHostelTypeId = db.jntuh_desirable_requirement_type.Where(l => l.isActive == true && l.isHostelRequirement == true)
                                                                            .Select(l => l.id)
                                                                            .ToArray();
            int campusHostelId = db.jntuh_college_hostel_maintenance.Where(a => a.collegeId == userCollegeID &&
                                                                                  campusHostelTypeId.Contains(a.requirementTypeID))
                                                                       .Select(a => a.id)
                                                                       .FirstOrDefault();
            if (campusHostelId != 0)
            {
                ViewBag.CampusHostel = submitted;
            }
            else
            {
                ViewBag.CampusHostel = notSubmitted;
            }

            #endregion

            #region Operational Funds

            int operationalFundsId = db.jntuh_college_funds.Where(funds => funds.collegeId == collegeId)
                                                           .Select(funds => funds.id)
                                                           .FirstOrDefault();

            if (operationalFundsId != 0)
            {
                ViewBag.OperationalFunds = submitted;
            }
            else
            {
                ViewBag.OperationalFunds = notSubmitted;
            }
            #endregion

            #region Income Details

            int incomeId = db.jntuh_college_income.Where(income => income.collegeId == collegeId)
                                                  .Select(income => income.id)
                                                  .FirstOrDefault();
            if (incomeId != 0)
            {
                ViewBag.IncomeDetails = submitted;
            }
            else
            {
                ViewBag.IncomeDetails = notSubmitted;
            }

            #endregion

            #region Expenditure Details

            int expenditureId = db.jntuh_college_expenditure.Where(expenditure => expenditure.collegeId == collegeId)
                                                            .Select(expenditure => expenditure.id)
                                                            .FirstOrDefault();
            if (expenditureId != 0)
            {
                ViewBag.ExpenditureDetails = submitted;
            }
            else
            {
                ViewBag.ExpenditureDetails = notSubmitted;
            }

            #endregion

            #region Placement Cell

            int placementId = db.jntuh_college_placement.Where(placement => placement.collegeId == collegeId)
                                                        .Select(placement => placement.id)
                                                        .FirstOrDefault();

            if (placementId != 0)
            {
                ViewBag.PlacementCell = submitted;
            }
            else
            {
                ViewBag.PlacementCell = notSubmitted;
            }

            #endregion

            #region College Photos

            int collegePhotosId = db.jntuh_college_document.Where(document => document.collegeId == collegeId)
                                                           .Select(document => document.id)
                                                           .FirstOrDefault();

            if (collegePhotosId != 0)
            {
                ViewBag.CollegePhotos = submitted;
            }
            else
            {
                ViewBag.CollegePhotos = notSubmitted;
            }

            #endregion

            #region Payment Details

            int paymentId = db.jntuh_college_payment.Where(payment => payment.collegeId == collegeId)
                                                    .Select(payment => payment.id)
                                                    .FirstOrDefault();
            if (paymentId != 0)
            {
                ViewBag.PaymentDetails = submitted;
            }
            else
            {
                ViewBag.PaymentDetails = notSubmitted;
            }

            #endregion

            #region Declaration

            bool declaration = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == collegeId)
                                                            .Select(editStatus => editStatus.IsCollegeEditable)
                                                            .FirstOrDefault();
            if (declaration == false)
            {
                ViewBag.Declaration = submitted;
            }
            else
            {
                ViewBag.Declaration = notSubmitted;
            }

            #endregion
            
            return View("~/Views/College/ValidateCollegeDataIndex.cshtml");
        }

    }
}
