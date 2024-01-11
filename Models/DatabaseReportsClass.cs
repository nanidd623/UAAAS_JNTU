using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class DatabaseReportsClass
    {

    }

    public class FacultyFieldsClass
    {
        public int? collegeId { get; set; }
        public int? academicYearId { get; set; }
        public bool AllItems { get; set; }
        public bool RegistrationNumber { get; set; }
        public bool Name { get; set; }
        public bool FathersName { get; set; }
        public bool MothersName { get; set; }
        public bool DateofBirth { get; set; }
        public bool Gender { get; set; }
        public bool Email { get; set; }
        public bool Mobile { get; set; }
        public bool PANNumber { get; set; }
        public bool AadhaarNumber { get; set; }
        public bool Photo { get; set; }
        public bool PANDocument { get; set; }
        public bool AadhaarDocument { get; set; }
        public bool Designation { get; set; }
        public bool Department { get; set; }
        public bool Specialization { get; set; }
        public bool GrossSalary { get; set; }
        public bool AICTEFacultyId { get; set; }
        public bool TotalExperience { get; set; }
        public bool PresentExperience { get; set; }
        public bool DateofAppointment { get; set; }
        public bool DeactivationReason { get; set; }
        public bool PHDTwoPagesFormat { get; set; }
        public bool Education { get; set; }
        public bool SSC { get; set; }
        public bool UG { get; set; }
        public bool PG { get; set; }
        public bool MPHIL { get; set; }
        public bool PHD { get; set; }
        public bool AllCertificates { get; set; }
        public HttpPostedFileBase RegNosExcel { get; set; }
        public int ColumnType { get; set; }
        public int CollegeType { get; set; }
        public List<FacultyDetailsClass> FacultyDetails { get; set; }

        public bool IdentifiedFor { get; set; }
        public bool HighestDegree { get; set; }
    }

    public class FacultyDetailsClass
    {
        public int? collegeId { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string FacultyPrincipalCase { get; set; }
        public string FacultyRegistrationNumber { get; set; }
        public string FacultyName { get; set; }
        public string FacultyFathersName { get; set; }
        public string FacultyMothersName { get; set; }
        public string FacultyDateofBirth { get; set; }
        public string FacultyGender { get; set; }
        public string FacultyEmail { get; set; }
        public string FacultyMobile { get; set; }
        public string FacultyPANNumber { get; set; }
        public string FacultyAadhaarNumber { get; set; }
        public string FacultyPhoto { get; set; }
        public string FacultyPANDocument { get; set; }
        public string FacultyAadhaarDocument { get; set; }
        public string FacultyDesignation { get; set; }
        public string FacultyOtherDesignation { get; set; }
        public string IdentifiedFor { get; set; }
        public int? DepartmentId { get; set; }
        public string FacultyDepartment { get; set; }
        public string FacultyOtherDepartment { get; set; }
        public int? SpecializationId { get; set; }
        public string FacultySpecialization { get; set; }
        public string FacultyGrossSalary { get; set; }
        public string FacultyAICTEFacultyId { get; set; }
        public int? FacultyTotalExperience { get; set; }
        public int? FacultyPresentExperience { get; set; }
        public string FacultyDateofAppointment { get; set; }
        public string FacultyDeactivationReason { get; set; }
        public string FacultyPHDTwoPagesFormat { get; set; }
        public string FacultyHighestDegree { get; set; }


        #region Faculty Education Details

        public string SSC_HallticketNo { get; set; }
        public string SSC_studiedEducation { get; set; }
        public string SSC_specialization { get; set; }
        public int? SSC_passedYear { get; set; }
        public decimal? SSC_percentage { get; set; }
        public int? SSC_division { get; set; }
        public string SSC_university { get; set; }
        public string SSC_place { get; set; }
        public string SSC_certificate { get; set; }

        public string UG_HallticketNo { get; set; }
        public string UG_studiedEducation { get; set; }
        public string UG_specialization { get; set; }
        public int? UG_passedYear { get; set; }
        public decimal? UG_percentage { get; set; }
        public int? UG_division { get; set; }
        public string UG_university { get; set; }
        public string UG_place { get; set; }
        public string UG_certificate { get; set; }

        public string PG_HallticketNo { get; set; }
        public string PG_studiedEducation { get; set; }
        public string PG_specialization { get; set; }
        public int? PG_passedYear { get; set; }
        public decimal? PG_percentage { get; set; }
        public int? PG_division { get; set; }
        public string PG_university { get; set; }
        public string PG_place { get; set; }
        public string PG_certificate { get; set; }

        public string MPhil_HallticketNo { get; set; }
        public string MPhil_studiedEducation { get; set; }
        public string MPhil_specialization { get; set; }
        public int? MPhil_passedYear { get; set; }
        public decimal? MPhil_percentage { get; set; }
        public int? MPhil_division { get; set; }
        public string MPhil_university { get; set; }
        public string MPhil_place { get; set; }
        public string MPhil_certificate { get; set; }

        public string PhD_HallticketNo { get; set; }
        public string PhD_studiedEducation { get; set; }
        public string PhD_specialization { get; set; }
        public int? PhD_passedYear { get; set; }
        public decimal? PhD_percentage { get; set; }
        public int? PhD_division { get; set; }
        public string PhD_university { get; set; }
        public string PhD_place { get; set; }
        public string PhD_certificate { get; set; }

        public string All_certificate { get; set; }
        #endregion
    }

    public class AffiliatedColleges
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string SubmissionDate { get; set; }
        public int AcademicYearId { get; set; }
        public string AffiliatedStatus { get; set; }
        public string ChairmanEmail { get; set; }
        public string ChairmanMobile { get; set; }
        public string CollegeEmail { get; set; }
        public string CollegeMobile { get; set; }
        public string SocietyEmail { get; set; }
        public string SocietyMobile { get; set; }
        public string SecretaryEmail { get; set; }
        public string SecretaryMobile { get; set; }
        public string PrincipalMobile { get; set; }
        public string PrincipalEmail { get; set; }
        public string PrincipalRegistrationNumber { get; set; }
        public string PrincipalName { get; set; }
        public string CollegeAddress { get; set; }
        public int DistrictId { get; set; }
    }

    public class ApprovedAdmittedIntake
    {
        public int? CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string OtherDepartment { get; set; }
        public string Specialization { get; set; }
        public int ShiftId { get; set; }

        public bool PresentYear { get; set; }
        public bool FirstYear { get; set; }
        public bool SecondYear { get; set; }
        public bool ThirdYear { get; set; }
        public bool FourYear { get; set; }
        public bool FifthYear { get; set; }
        public bool SixYear { get; set; }

        public bool AICTEApprovedIntake { get; set; }
        public bool ExamsAdmittedIntakeR { get; set; }
        public bool ExamsAdmittedIntakeL { get; set; }
        public bool JntuApprovedIntake { get; set; }
        public bool CollegeAdmittedIntake { get; set; }
       
        public int? ProposedIntake { get; set; }

        public int PresentYearAICTEApproved { get; set; }
        public int FirstYearAICTEApproved { get; set; }
        public int SecondYearAICTEApproved { get; set; }
        public int ThirdYearAICTEApproved { get; set; }
        public int FourYearAICTEApproved { get; set; }
        public int FifthYearAICTEApproved { get; set; }
        public int SixYearAICTEApproved { get; set; }

        public int PresentYearJntuApproved { get; set; }
        public int FirstYearJntuApproved { get; set; }
        public int SecondYearJntuApproved { get; set; }
        public int ThirdYearJntuApproved { get; set; }
        public int FourYearJntuApproved { get; set; }
        public int FifthYearJntuApproved { get; set; }
        public int SixYearJntuApproved { get; set; }

        public int PresentYearCollegeAdmitted { get; set; }
        public int FirstYearCollegeAdmitted { get; set; }
        public int SecondYearCollegeAdmitted { get; set; }
        public int ThirdYearCollegeAdmitted { get; set; }
        public int FourYearCollegeAdmitted { get; set; }
        public int FifthYearCollegeAdmitted { get; set; }
        public int SixYearCollegeAdmitted { get; set; }

        public int PresentYearExamsBranchRegularAdmitted { get; set; }
        public int FirstYearExamsBranchRegularAdmitted { get; set; }
        public int SecondYearExamsBranchRegularAdmitted { get; set; }
        public int ThirdYearExamsBranchRegularAdmitted { get; set; }
        public int FourYearExamsBranchRegularAdmitted { get; set; }
        public int FifthYearExamsBranchRegularAdmitted { get; set; }
        public int SixYearExamsBranchRegularAdmitted { get; set; }

        public int PresentYearExamsBranchLateralAdmitted { get; set; }
        public int FirstYearExamsBranchLateralAdmitted { get; set; }
        public int SecondYearExamsBranchLateralAdmitted { get; set; }
        public int ThirdYearExamsBranchLateralAdmitted { get; set; }
        public int FourYearExamsBranchLateralAdmitted { get; set; }
        public int FifthYearExamsBranchLateralAdmitted { get; set; }
        public int SixYearExamsBranchLateralAdmitted { get; set; }
    }

    public class FacultyFlags
    {
        public int? collegeId { get; set; }
        public bool AllItems { get; set; }
        public bool type { get; set; }
        public bool absent { get; set; }
        public bool OriginalCertificatesNotShown{ get; set; }
        public bool Xeroxcopyofcertificates { get; set; }
        public bool NotQualifiedAsperAICTE { get; set; }
        public bool NoSCM { get; set; }
        public bool PANNumber { get; set; }
        public bool IncompleteCertificates { get; set; }
        public bool Blacklistfaculy { get; set; }
        public bool NoRelevantUG { get; set; }
        public bool NoRelevantPG { get; set; }
        public bool NORelevantPHD { get; set; }
        public bool InvalidPANNumber { get; set; }
        public bool OriginalsVerifiedPHD { get; set; }
        public bool OriginalsVerifiedUG { get; set; }
        public bool Invaliddegree { get; set; }
        public bool BAS { get; set; }
        public bool InvalidAadhaar { get; set; }
        public bool FakePhD { get; set; }
        public bool Noclass { get; set; }
        public bool AbsentforVerification { get; set; }
        public bool NotConsideredPhD { get; set; }
        public bool NoPGspecialization { get; set; }
        public bool Genuinenessnotsubmitted { get; set; }
        public bool NoForm16 { get; set; }
        public bool NoForm26AS { get; set; }
        public bool Covid19 { get; set; }
        public bool Maternity { get; set; }

        public List<FlagsNames> FacultyFlagsDetails { get; set; }
    }

    public class FlagsNames
    {
        public int? collegeId { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }
        public string typeFlag { get; set; }
        public string absentFlag { get; set; }
        public string OriginalCertificatesNotShownFlag { get; set; }
        public string XeroxcopyofcertificatesFlag { get; set; }
        public string NotQualifiedAsperAICTEFlag { get; set; }
        public string NoSCMFlag { get; set; }
        public string PANNumberFlag { get; set; }
        public string IncompleteCertificatesFlag { get; set; }
        public string BlacklistfaculyFlag { get; set; }
        public string NoRelevantUGFlag { get; set; }
        public string NoRelevantPGFlag { get; set; }
        public string NORelevantPHDFlag { get; set; }
        public string InvalidPANNumberFlag { get; set; }
        public string OriginalsVerifiedPHDFlag { get; set; }
        public string OriginalsVerifiedUGFlag { get; set; }
        public string InvaliddegreeFlag { get; set; }
        public string BASFlag { get; set; }
        public string InvalidAadhaarFlag { get; set; }
        public string FakePhDFlag { get; set; }
        public string NoclassFlag { get; set; }
        public string AbsentforVerificationFlag { get; set; }
        public string NotConsideredPhDFlag { get; set; }
        public string NoPGspecializationFlag { get; set; }
        public string GenuinenessnotsubmittedFlag { get; set; }
        public string NoForm16 { get; set; }
        public string NoForm26AS { get; set; }
        public string Covid19 { get; set; }
        public string Maternity { get; set; }
    }
}