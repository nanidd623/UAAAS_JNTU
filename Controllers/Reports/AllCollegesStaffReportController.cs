using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AllCollegesStaffReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private int TeachingId = 0;
        private int NonTeachingId = 0;
        private int ProfessorId = 0;
        private int AssociateProfessorId = 0;
        private int AssistantProfessorId = 0;
        private int OtherId = 0;
        private int[] BTechDepartmentId;
        private int[] BPharmacyDepartmentId;
        private int[] MTechDepartmentId;
        private int[] MPharmacyId;
        private int[] MCADepartmentId;
        private int[] MBADepartmentId;
        private int[] MAMDepartmentId;
        private int[] MTMDepartmentId;
        private int[] PharmdDepartmentId;
        private int[] PharmdPBDepartmentId;
        private int[] CollegeTypeDepartmentId;
        private int CollegeTypeId=0;
        private void GetId()
        {
            TeachingId = db.jntuh_faculty_type.Where(f => f.facultyType == "Teaching").Select(f => f.id).FirstOrDefault();
            NonTeachingId = db.jntuh_faculty_type.Where(f => f.facultyType == "Non-Teaching").Select(f => f.id).FirstOrDefault();
            ProfessorId = db.jntuh_designation.Where(f => f.designation == "Professor").Select(f => f.id).FirstOrDefault();
            AssociateProfessorId = db.jntuh_designation.Where(f => f.designation == "Associate Professor").Select(f => f.id).FirstOrDefault();
            AssistantProfessorId = db.jntuh_designation.Where(f => f.designation == "Assistant Professor").Select(f => f.id).FirstOrDefault();
            OtherId = db.jntuh_designation.Where(f => f.designation == "Other").Select(f => f.id).FirstOrDefault();
            //BTechDepartmentId=(from Degree in db.jntuh_degree
            //                   join Department in db.jntuh_department on Degree.id equals Department.degreeId
            //                   where (Degree.isActive==true && Department.isActive==true && Degree.degree=="B.Tech")
            //                   select
            //                      )
        }

        private List<AllCollegesStaffReport> AllCollegesStaff(string cmd)
        {
            decimal RatifiedCount = 0;
            if (cmd == "Engineering")
            {
                CollegeTypeId = db.jntuh_college_type
                                .Where(c => c.collegeType.Trim() == "Engineering")
                                .Select(c => c.id)
                                .FirstOrDefault();
            }
            else if (cmd == "Pharmacy")
            {
                CollegeTypeId = db.jntuh_college_type
                                .Where(c => c.collegeType.Trim() == "Pharmacy")
                                .Select(c => c.id)
                                .FirstOrDefault();
            }
            else if (cmd == "Standalone")
            {
                CollegeTypeId = db.jntuh_college_type
                                .Where(c => c.collegeType.Trim() == "Standalone")
                                .Select(c => c.id)
                                .FirstOrDefault();
            }
            else if (cmd == "Integrated Campus")
            {
                CollegeTypeId = db.jntuh_college_type
                                .Where(c => c.collegeType.Trim() == "Integrated Campus")
                                .Select(c => c.id)
                                .FirstOrDefault();
            }
            else if (cmd == "Technical Campus")
            {
                CollegeTypeId = db.jntuh_college_type
                                .Where(c => c.collegeType.Trim() == "Technical Campus")
                                .Select(c => c.id)
                                .FirstOrDefault();
            }
            List<AllCollegesStaffReport> AllCollegesStaffDetails = new List<AllCollegesStaffReport>();
            AllCollegesStaffDetails = db.jntuh_college
                                        .Where(c => c.isActive == true &&
                                                    c.collegeTypeID == CollegeTypeId)
                                        .Select(c => new AllCollegesStaffReport
                                        {
                                            CollegeId=c.id,
                                            CollegeCode=c.collegeCode,
                                            CollegeName=c.collegeName
                                        }).ToList();
            foreach (var item in AllCollegesStaffDetails)
            {
                RatifiedCount = 0;
                item.DistrictId = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == item.CollegeId).Select(a => a.districtId).FirstOrDefault();
                item.DistrictName = db.jntuh_district.Where(d => d.id == item.DistrictId).Select(d => d.districtName).FirstOrDefault();
                List<jntuh_college_faculty> CollegeFaculty = db.jntuh_college_faculty.Where(f => f.collegeId == item.CollegeId && (f.facultyTypeId==TeachingId || f.facultyTypeId==NonTeachingId)).ToList();
                if (CollegeFaculty != null)
                {
                    item.Teaching = CollegeFaculty.Where(f => f.facultyTypeId == TeachingId).Select(f => f.id).Count();
                    item.NonTeaching = CollegeFaculty.Where(f => f.facultyTypeId == NonTeachingId).Select(f => f.id).Count();
                    item.Total = item.Teaching + item.NonTeaching;
                    RatifiedCount=CollegeFaculty.Where(f => f.isFacultyRatifiedByJNTU==true).Select(f => f.id).Count();
                    if (item.Total != 0 && RatifiedCount != 0)
                    {
                        item.Ratified = (RatifiedCount / item.Total) * 100;
                        item.Ratified = Math.Round(item.Ratified, 2);
                    }
                    item.Professors = CollegeFaculty.Where(f => f.facultyDesignationId == ProfessorId).Select(f => f.id).Count();
                    item.AssociateProfessors = CollegeFaculty.Where(f => f.facultyDesignationId == AssociateProfessorId).Select(f => f.id).Count();
                    item.AssistantProfessors = CollegeFaculty.Where(f => f.facultyDesignationId == AssistantProfessorId).Select(f => f.id).Count();
                    item.Others = CollegeFaculty.Where(f => f.facultyDesignationId == OtherId).Select(f => f.id).Count();
                }
            }
            return AllCollegesStaffDetails;
        }
        public ActionResult AllCollegesStaffReport()
        {
            return View();
        }

    }
}
