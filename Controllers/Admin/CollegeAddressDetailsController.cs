using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Admin
{
    [ErrorHandling]
    public class CollegeAddressDetailsController : BaseController
    {
        private readonly uaaasDBContext _db = new uaaasDBContext();
        // GET: /CollegeAddressDetails/

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var collegeIds = _db.jntuh_college_intake_existing.Where(i => i.collegeId != 375 && (i.academicYearId == 11 || i.academicYearId == 10 || i.academicYearId == 9) && i.approvedIntake != 0).OrderBy(i => i.collegeId).Select(i => i.collegeId).Distinct().ToArray();
            //var collegeIds = _db.jntuh_college.Where(e => e.isActive && e.id != 375).Select(e => e.id).Distinct().ToArray();
            var collegeNameAndAddress = (from coll in _db.jntuh_college
                                         join addr in _db.jntuh_address on coll.id equals addr.collegeId
                                         join distr in _db.jntuh_district on addr.districtId equals distr.id
                                         join stat in _db.jntuh_state on addr.stateId equals stat.id
                                         where collegeIds.Contains(coll.id) && addr.addressTye == "COLLEGE"
                                         select new CollegeAddress
                                         {
                                             Id = coll.id,
                                             CollegeName = coll.collegeName,
                                             CollegeCode = coll.collegeCode,
                                             EamcetCode = coll.eamcetCode,
                                             Address = addr.address,
                                             Email = addr.email,
                                             Landline = addr.landline,
                                             Website = addr.website,
                                             TownorCity = addr.townOrCity,
                                             Mandal = addr.mandal,
                                             Pincode = addr.pincode,
                                             Fax = addr.fax,
                                             Mobile = addr.mobile,
                                             District = distr.districtName,
                                             State = stat.stateName,
                                             //Issubmitted = edit.isSubmitted
                                         }).ToList();
            return View(collegeNameAndAddress);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ViewIntake(int id)
        {
            if (id <= 0) return View(new List<CollegeIntakeExisting>());
            var userCollegeDetails = _db.jntuh_college.FirstOrDefault(u => u.id == id);
            if (userCollegeDetails == null) return View(new List<CollegeIntakeExisting>());
            var userCollegeId = userCollegeDetails.id;
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var actualYear = _db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            var jntuhAcademicYear = _db.jntuh_academic_year;
            ViewBag.AcademicYear = jntuhAcademicYear.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.academicYear).FirstOrDefault();
            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1), (actualYear + 2).ToString().Substring(2, 2));
            var ay0 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2), (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3), (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4), (actualYear - 3).ToString().Substring(2, 2));
            var presentYear = jntuhAcademicYear.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            var ay1 = jntuhAcademicYear.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            var ay2 = jntuhAcademicYear.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            var ay3 = jntuhAcademicYear.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            var ay4 = jntuhAcademicYear.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            var ay5 = jntuhAcademicYear.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();
            //var enclosureId = _db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
            //var aicteApprovalLettr = _db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeId).Select(e => e.path).FirstOrDefault();
            var inactivespids = _db.jntuh_specialization.Where(s => s.isActive == false).Select(s => s.id).ToArray();
            //int[] academicyearids = { AY1, AY2, AY3, AY4, AY5 };
            var intake = _db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeId && !inactivespids.Contains(i.specializationId)).ToList();
            var collegeIntakeExisting = new List<CollegeIntakeExisting>();
            var jntuhSpecialization = _db.jntuh_specialization;
            var jntuhDepartment = _db.jntuh_department;
            var jntuhDegree = _db.jntuh_degree;
            var jntuhShift = _db.jntuh_shift;
            //var jntuhCollegeNocData = _db.jntuh_college_noc_data.Where(n => n.collegeId == userCollegeId && n.isClosure == true).Select(s => s).ToList();
            foreach (var item in intake)
            {
                var newIntake = new CollegeIntakeExisting
                {
                    id = item.id,
                    isClosed = false,
                    collegeId = item.collegeId,
                    academicYearId = item.academicYearId,
                    shiftId = item.shiftId,
                    isActive = item.isActive,
                    //nbaFrom = item.nbaFrom,
                    //nbaTo = item.nbaTo,
                    specializationId = item.specializationId,
                    Specialization = jntuhSpecialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault(),
                    DepartmentID = jntuhSpecialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault()
                };
                newIntake.Department = jntuhDepartment.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = jntuhDepartment.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree = jntuhDegree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder = jntuhDegree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = jntuhShift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                //newIntake.AICTEApprovalLettr = aicteApprovalLettr;
                //var collegecid = jntuhCollegeNocData.Where(d => d.noctypeId == 9).Select(s => s.id).FirstOrDefault();
                //if (collegecid == 0)
                //{
                //    var closedid = jntuhCollegeNocData.Where(d => d.specializationId == item.specializationId && d.shiftId == item.shiftId && d.remarks == null).Select(s => s.id).FirstOrDefault();
                //    if (closedid != 0)
                //    {
                //        newIntake.isClosed = true;
                //    }
                //}
                //else
                //{
                //    newIntake.isClosed = true;
                //}
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            var jntuhCollegeIntakeExisting = _db.jntuh_college_intake_existing.AsNoTracking().Where(e => e.collegeId == userCollegeId && e.academicYearId == ay0).ToList();
            var nbaaccrdata = _db.jntuh_college_nbaaccreditationdata.AsNoTracking().Where(i => i.collegeid == userCollegeId).ToList();
            foreach (var item in collegeIntakeExisting)
            {
                var nbadt = nbaaccrdata.FirstOrDefault(i => i.specealizationid == item.specializationId && i.accademicyear == ay0);
                if (nbadt != null && nbadt.nbafrom != null)
                    item.nbaFromDate = nbadt.nbafrom.ToString("dd/MM/yyyy");
                if (nbadt != null && nbadt.nbato != null)
                    item.nbaToDate = nbadt.nbato.ToString("dd/MM/yyyy");
                if (nbadt != null) item.UploadNBAApproveLetter = nbadt.nbaapprovalletter;
                var details = jntuhCollegeIntakeExisting.Where(e => e.specializationId == item.specializationId && e.shiftId == item.shiftId).Select(e => e).FirstOrDefault();
                if (details != null)
                {
                    item.ApprovedIntake = details.approvedIntake;
                    item.letterPath = details.approvalLetter;
                    item.ProposedIntake = details.proposedIntake;
                    item.courseStatus = details.courseStatus;
                }
                item.AICTEapprovedIntake1 = GetIntake(userCollegeId, ay1, item.specializationId, item.shiftId, 2);
                item.approvedIntake1 = GetIntake(userCollegeId, ay1, item.specializationId, item.shiftId, 1);
                item.admittedIntake1 = GetIntake(userCollegeId, ay1, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake2 = GetIntake(userCollegeId, ay2, item.specializationId, item.shiftId, 2);
                item.approvedIntake2 = GetIntake(userCollegeId, ay2, item.specializationId, item.shiftId, 1);
                item.admittedIntake2 = GetIntake(userCollegeId, ay2, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake3 = GetIntake(userCollegeId, ay3, item.specializationId, item.shiftId, 2);
                item.approvedIntake3 = GetIntake(userCollegeId, ay3, item.specializationId, item.shiftId, 1);
                item.admittedIntake3 = GetIntake(userCollegeId, ay3, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake4 = GetIntake(userCollegeId, ay4, item.specializationId, item.shiftId, 2);
                item.approvedIntake4 = GetIntake(userCollegeId, ay4, item.specializationId, item.shiftId, 1);
                item.admittedIntake4 = GetIntake(userCollegeId, ay4, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake5 = GetIntake(userCollegeId, ay5, item.specializationId, item.shiftId, 2);
                item.approvedIntake5 = GetIntake(userCollegeId, ay5, item.specializationId, item.shiftId, 1);
                item.admittedIntake5 = GetIntake(userCollegeId, ay5, item.specializationId, item.shiftId, 0);
            }
            collegeIntakeExisting = collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            ViewBag.ExistingIntake = collegeIntakeExisting;
            ViewBag.Count = collegeIntakeExisting.Count();
            ViewBag.collegeName = userCollegeDetails.collegeName;
            ViewBag.collegeCode = userCollegeDetails.collegeCode;
            return View(collegeIntakeExisting);
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake;
            switch (flag)
            {
                case 1:
                    intake = _db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                    break;
                case 2:
                    intake = _db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();
                    break;
                case 3:
                    intake = _db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntakeasperExambranch_R).FirstOrDefault();
                    break;
                case 4:
                    intake = _db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntakeasperExambranch_L).FirstOrDefault();
                    break;
                default:
                    intake = _db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
                    break;
            }
            return intake;
        }
    }
    public class CollegeAddress
    {
        public int Id { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public string EamcetCode { get; set; }
        public string Email { get; set; }
        public string Landline { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string TownorCity { get; set; }
        public string Mandal { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public int Pincode { get; set; }
        public string Fax { get; set; }
        public string Mobile { get; set; }
        public bool? Issubmitted { get; set; }

    }
}
