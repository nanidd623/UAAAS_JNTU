using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class AllCollegesIntakeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AllCollegesIntake()
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeIntakeExisting> intakedetailsList = new List<CollegeIntakeExisting>();
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();

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
                newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = item.jntuh_shift.shiftName;
                collegeIntakeExisting.Add(newIntake);
            }
            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

            foreach (var item in collegeIntakeExisting)
            {
                CollegeIntakeExisting intakedetails = new CollegeIntakeExisting();
                intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                intakedetails.Degree = item.Degree;
                intakedetails.collegeId = item.collegeId;
                intakedetails.Department = item.Department;
                intakedetails.Specialization = item.Specialization;
                intakedetails.shiftId = item.shiftId;
                intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;
                intakedetailsList.Add(intakedetails);

            }
            intakedetailsList = intakedetailsList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=College Wise Intake.xls");
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/Reports/_AllCollegesIntake.cshtml", intakedetailsList);

        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AllCollegesIntakeWithLabs(int? collegeId, string type)
        {
            var actualYear1 =
                 db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
             .Select(a => a.actualYear)
             .FirstOrDefault();
            var AcademicYearId1 =
                db.jntuh_academic_year.Where(d => d.actualYear == (actualYear1 + 1)).Select(z => z.id).FirstOrDefault();

            var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == AcademicYearId1 && c.e.IsCollegeEditable == false).Select(c => new
            {
                collegeId = c.co.id,
                collegeName = c.co.collegeCode + "-" + c.co.collegeName
            }).OrderBy(c => c.collegeName).ToList();

            colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            int[] AppealCollegeIds = db.jntuh_appeal_college_edit_status.Select(C => C.collegeId).ToArray();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegesIntakeWithLabs> intakedetailsList = new List<CollegesIntakeWithLabs>();
            if (collegeId != null)
            {
                int[] collegeIDs = null;
                if (collegeId != 0)
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                }
                else
                {
                    collegeIDs = db.jntuh_college.Where(c => AppealCollegeIds.Contains(c.id) && c.isActive == true).Select(c => c.id).ToArray();
                }
                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

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
                    newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                    newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                    newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                    newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    collegeIntakeExisting.Add(newIntake);
                }
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();
                var jntuh_lab_master = db.jntuh_lab_master.ToList();
               
                // Old Code Commented By Srinivas.T
                //var jntuh_college_laboratories = db.jntuh_college_laboratories.ToList();
                //var jntuh_colleg = db.jntuh_college.ToList();

                var jntuh_college_laboratories = db.jntuh_college_laboratories.Where(C => collegeIDs.Contains(C.CollegeID)).ToList();
                var jntuh_colleg = db.jntuh_college.Where(C => collegeIDs.Contains(C.id)).ToList();

                int SpecializationId = 0;
                int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
                int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
                if (DegreeIds.Contains(4))
                    SpecializationId = 39;
                else
                    SpecializationId = 0;
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                int DegreeID = 0,Year=0;
                foreach (var item in collegeIntakeExisting)
                {
                    CollegesIntakeWithLabs intakedetails = new CollegesIntakeWithLabs();
                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    intakedetails.Degree = item.Degree;
                    intakedetails.DegreeID = item.degreeID;
                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_colleg.Where(c => c.isActive == true && c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_colleg.Where(c => c.isActive == true && c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.shiftId = item.shiftId;
                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;


                    #region Old code Commented By Srinivas
                    //if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
                    //{
                    //    intakedetails.requiredLabs41 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 1).Count();
                    //    intakedetails.uplodedLabs41 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 1).Count();
                    //    intakedetails.notuplodedLabs41 = intakedetails.requiredLabs41 - intakedetails.uplodedLabs41;

                    //    intakedetails.requiredLabs42 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 2).Count();
                    //    intakedetails.uplodedLabs42 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 2).Count();
                    //    intakedetails.notuplodedLabs42 = intakedetails.requiredLabs42 - intakedetails.uplodedLabs42;

                    //    intakedetails.requiredLabs31 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 1).Count();
                    //    intakedetails.uplodedLabs31 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 1).Count();
                    //    intakedetails.notuplodedLabs31 = intakedetails.requiredLabs31 - intakedetails.uplodedLabs31;

                    //    intakedetails.requiredLabs32 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 2).Count();
                    //    intakedetails.uplodedLabs32 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 2).Count();
                    //    intakedetails.notuplodedLabs32 = intakedetails.requiredLabs32 - intakedetails.uplodedLabs32;

                    //    intakedetails.requiredLabs21 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 1).Count();
                    //    intakedetails.uplodedLabs21 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 1).Count();
                    //    intakedetails.notuplodedLabs21 = intakedetails.requiredLabs21 - intakedetails.uplodedLabs21;

                    //    intakedetails.requiredLabs22 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 2).Count();
                    //    intakedetails.uplodedLabs22 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 2).Count();
                    //    intakedetails.notuplodedLabs22 = intakedetails.requiredLabs22 - intakedetails.uplodedLabs22;


                    //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1).Count();
                    //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).Count();
                    //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;

                    //    intakedetails.requiredLabs12 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 2).Count();
                    //    intakedetails.uplodedLabs12 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 2).Count();
                    //    intakedetails.notuplodedLabs12 = intakedetails.requiredLabs12 - intakedetails.uplodedLabs12;

                    //}
                    //else
                    //{
                    //    intakedetails.requiredLabs41 = 0;
                    //    intakedetails.uplodedLabs41 = 0;
                    //    intakedetails.notuplodedLabs41 = 0;

                    //    intakedetails.requiredLabs42 = 0;
                    //    intakedetails.uplodedLabs42 = 0;
                    //    intakedetails.notuplodedLabs42 = 0;

                    //    intakedetails.requiredLabs31 = 0;
                    //    intakedetails.uplodedLabs31 = 0;
                    //    intakedetails.notuplodedLabs31 = 0;

                    //    intakedetails.requiredLabs32 = 0;
                    //    intakedetails.uplodedLabs32 = 0;
                    //    intakedetails.notuplodedLabs32 = 0;

                    //    intakedetails.requiredLabs21 = 0;
                    //    intakedetails.uplodedLabs21 = 0;
                    //    intakedetails.notuplodedLabs21 = 0;

                    //    intakedetails.requiredLabs22 = 0;
                    //    intakedetails.uplodedLabs22 = 0;
                    //    intakedetails.notuplodedLabs22 = 0;

                    //    intakedetails.requiredLabs11 = 4;
                    //    //intakedetails.uplodedLabs11 = 0;
                    //    //&& l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1
                    //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId).Count();
                    //    intakedetails.notuplodedLabs11 = 4 - intakedetails.uplodedLabs11;

                    //    intakedetails.requiredLabs12 = 0;
                    //    intakedetails.uplodedLabs12 = 0;
                    //    intakedetails.notuplodedLabs12 = 0;
                    //}
                    //intakedetailsList.Add(intakedetails);
                    #endregion

                    #region New code Written By Srinivas
                    int[] degids = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId).Select(l => l.jntuh_lab_master.DegreeID).Distinct().ToArray();
                    DegreeID = item.degreeID;
                  
                    Year = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId).Select(l => l.Year).FirstOrDefault();
                    if (CollegeAffiliationStatus == "Yes")
                    {
                        intakedetails.requiredLabs41 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs41 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                       // intakedetails.Autonomus41 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs41 = intakedetails.requiredLabs41 - intakedetails.uplodedLabs41;

                        intakedetails.requiredLabs42 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 2 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs42 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 2 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                        //intakedetails.Autonomus42 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 2 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs42 = intakedetails.requiredLabs42 - intakedetails.uplodedLabs42;

                        intakedetails.requiredLabs31 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs31 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                       // intakedetails.Autonomus31 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs31 = intakedetails.requiredLabs31 - intakedetails.uplodedLabs31;

                        intakedetails.requiredLabs32 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 2 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs32 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 2 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                       // intakedetails.Autonomus32 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 2 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs32 = intakedetails.requiredLabs32 - intakedetails.uplodedLabs32;

                        intakedetails.requiredLabs21 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs21 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                       // intakedetails.Autonomus21 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs21 = intakedetails.requiredLabs21 - intakedetails.uplodedLabs21;

                        intakedetails.requiredLabs22 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 2 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs22 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 2).GroupBy(l => l.EquipmentID).Count();
                      //  intakedetails.Autonomus22 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 2 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs22 = intakedetails.requiredLabs22 - intakedetails.uplodedLabs22;


                        //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode != "TMP-CL").Count();
                        //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).Count();
                        //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;

                        if (DegreeID == 1)
                        {
                            //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 1 && l.SpecializationID == item.specializationId && l.CollegeId == item.collegeId)).Count();
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || (l.jntuh_lab_master.Year == 0 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.SpecializationID == item.specializationId)).GroupBy(l => l.EquipmentID).Count(); 
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count(); 

                           // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL" || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 1 && l.SpecializationID == item.specializationId && l.Labcode == "TMP-CL")).Count();  //  && l.Year == 1 && l.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 2)
                        {
                            //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 2 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 2 && l.SpecializationID == item.specializationId && l.CollegeId == item.collegeId)).Count(); // || (l.Year == 1 || l.Semester == 1)   && l.Year == 1 && l.Semester == 1
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1  || (l.jntuh_lab_master.Year == 0 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.SpecializationID == item.specializationId)).GroupBy(l => l.EquipmentID).Count(); //|| (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)  && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 2 && l.Year ==1 && l.Semester ==1 && l.CollegeId == item.collegeId).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1&&l.jntuh_lab_master.CollegeId==item.collegeId).GroupBy(l => l.EquipmentID).Count(); 

                           // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 2 && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL" || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 2 && l.SpecializationID == item.specializationId && l.Labcode == "TMP-CL")).Count(); // || (l.Year == 1 || l.Semester == 1)   && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 3)
                        {

                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.DegreeID == 3 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 3).GroupBy(l => l.EquipmentID).Count();
                           // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.DegreeID == 3 && l.Labcode == "TMP-CL").Count();
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 4)
                        {
                            //if (Year == 1)
                            //{
                            //     intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 || l.SpecializationID == 39).Count();
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            //else
                            //{
                            //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1).Count();
                            //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}


                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == 39 && l.CollegeId == item.collegeId).Count();  //l.SpecializationID == item.specializationId &&  l.Semester == 1 && 
                           // intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.Year == 1  && l.SpecializationID == 39).Count(); 
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.Year == 1 || l.jntuh_lab_master.Year == 3&&l.jntuh_lab_master.Semester==0 && l.jntuh_lab_master.SpecializationID == 39 && l.jntuh_lab_master.Labcode != "TMP-CL").GroupBy(l => l.EquipmentID).Count();   // && l.jntuh_lab_master.SpecializationID == item.specializationId  l.jntuh_lab_master.Semester == 1 &&
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == 39 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();   // && l.jntuh_lab_master.Year == 1 || l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 0
                            //intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == 39 && l.Year == 1 && l.Labcode == "TMP-CL").Count();
                          //new  intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID==item.specializationId &&l.Labcode == "TMP-CL" && l.Year == 0 && l.Semester == 0).Count();

                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;


                            //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.Year == 1 && l.Semester == 0 && l.Labcode != "TMP-CL" && l.SpecializationID == 39).Count();
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.Labcode != "TMP-CL" && l.jntuh_lab_master.SpecializationID == 39).Count();
                            //intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                            //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;


                            //if (Year == 1)
                            //{
                            //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l =>  l.Year == 1 && l.Semester == 1 && l.Labcode != "TMP-CL" && l.SpecializationID == 39).Count();
                            //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.Labcode != "TMP-CL" || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //    intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                            //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            //else
                            //{
                            //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode != "TMP-CL" ).Count();
                            //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.Labcode != "TMP-CL" ).Count();
                            //    intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                            //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            

                        }
                        else if (DegreeID == 5)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 5 && l.CollegeId == item.collegeId).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 5 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                           // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 5 && l.Labcode == "TMP-CL").Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 6)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 6 && l.CollegeId == item.collegeId && l.Year==1 && l.Semester==1).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 6 && l.jntuh_lab_master.CollegeId == item.collegeId && l.jntuh_lab_master.Year==1 && l.jntuh_lab_master.Semester==1).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                          //  intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 6 && l.Labcode == "TMP-CL").Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 9)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 9 && l.CollegeId == item.collegeId && l.Year == 1 && l.Semester == 1).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 9 && l.jntuh_lab_master.CollegeId == item.collegeId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                           // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 9 && l.Labcode == "TMP-CL").Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 10)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 10 && l.CollegeId == item.collegeId).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 10 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                           // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 510 && l.Labcode == "TMP-CL").Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }


                        intakedetails.requiredLabs12 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 2 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs12 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 2 && l.jntuh_lab_master.CollegeId==item.collegeId).GroupBy(l => l.EquipmentID).Count();
                      //  intakedetails.Autonomus12 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 2 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs12 = intakedetails.requiredLabs12 - intakedetails.uplodedLabs12;

                       // intakedetailsList.Add(intakedetails);
                    }
                    else if ((CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null))
                    {
                        intakedetails.requiredLabs41 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 1 && l.CollegeId ==null).Count();
                        intakedetails.uplodedLabs41 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs41 = intakedetails.requiredLabs41 - intakedetails.uplodedLabs41;

                        intakedetails.requiredLabs42 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 2 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs42 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 2).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs42 = intakedetails.requiredLabs42 - intakedetails.uplodedLabs42;

                        intakedetails.requiredLabs31 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 1 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs31 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs31 = intakedetails.requiredLabs31 - intakedetails.uplodedLabs31;

                        intakedetails.requiredLabs32 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 2 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs32 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 2).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs32 = intakedetails.requiredLabs32 - intakedetails.uplodedLabs32;

                        intakedetails.requiredLabs21 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 1 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs21 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs21 = intakedetails.requiredLabs21 - intakedetails.uplodedLabs21;

                        intakedetails.requiredLabs22 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 2 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs22 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 2).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs22 = intakedetails.requiredLabs22 - intakedetails.uplodedLabs22;


                        //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.CollegeId == null).Count();
                        //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;

                        if (DegreeID == 1)
                        {
                            //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.CollegeId == null || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 1 && l.SpecializationID == item.specializationId && l.CollegeId == null)).Count();  
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || (l.jntuh_lab_master.Year == 0 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.SpecializationID == item.specializationId)).GroupBy(l => l.EquipmentID).Count();
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.CollegeId == null).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.CollegeId==null).GroupBy(l => l.EquipmentID).Count();
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 2)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 2 && l.Year == 1 && l.Semester == 1 && l.CollegeId == null || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 2 && l.SpecializationID == item.specializationId && l.CollegeId == null)).Count(); // || (l.Year == 1 || l.Semester == 1)   && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || (l.jntuh_lab_master.Year == 0 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.SpecializationID == item.specializationId)).GroupBy(l => l.EquipmentID).Count(); //|| (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)  && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 3)
                        {

                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.DegreeID == 3 && l.Year == 1 && l.Semester == 1 && l.CollegeId == null).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 3).GroupBy(l => l.EquipmentID).Count();
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 4)
                        {
                            //if (Year == 1)
                            //{
                            //     intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 || l.SpecializationID == 39).Count();
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            //else
                            //{
                            //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1).Count();
                            //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == 39 && l.CollegeId == null).Count();  //l.SpecializationID == item.specializationId &&  l.Semester == 1 && 
                          //  intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.Year == 1 && l.SpecializationID == 39).Count();  //l.SpecializationID == item.specializationId &&  l.Semester == 1 && 
                           // intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.Year == 1  && l.jntuh_lab_master.SpecializationID == 39).GroupBy(l => l.EquipmentID).Count();   // && l.jntuh_lab_master.SpecializationID == item.specializationId  l.jntuh_lab_master.Semester == 1 &&
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId &&  l.jntuh_lab_master.SpecializationID == 39 && l.jntuh_lab_master.CollegeId==null).GroupBy(l => l.EquipmentID).Count();   // && l.jntuh_lab_master.SpecializationID == item.specializationId  l.jntuh_lab_master.Semester == 1 &&
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                           
                        }
                        else if (DegreeID == 5)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 5 && l.CollegeId == null).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 5).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 6)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 6 && l.CollegeId == null && l.Year == 1 && l.Semester == 1).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 6 && l.jntuh_lab_master.CollegeId==null&& l.jntuh_lab_master.Year==1 && l.jntuh_lab_master.Semester==1).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 9)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 9 && l.CollegeId == null).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 9).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 10)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 10 && l.CollegeId == null).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 10).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }


                        intakedetails.requiredLabs12 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 2 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs12 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 2 && l.jntuh_lab_master.CollegeId==null).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs12 = intakedetails.requiredLabs12 - intakedetails.uplodedLabs12;

                       
                    }
                    intakedetailsList.Add(intakedetails);
                   
                    #endregion
                    
                   

                }
                intakedetailsList = intakedetailsList.OrderBy(ei => ei.collegeName).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
                if (type == "Excel")
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=College Wise Intake.xls");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/Reports/_AllCollegesIntakeWithLabs.cshtml", intakedetailsList);
                }
            }
            return View("~/Views/Reports/AllCollegesIntakeWithLabs.cshtml", intakedetailsList);
        }

       [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AllCollegesIntakeWithLabsDetailsNew(int? collegeId)
        {
            var actualYear1 =
                   db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
               .Select(a => a.actualYear)
               .FirstOrDefault();
            var AcademicYearId1 =
                db.jntuh_academic_year.Where(d => d.actualYear == (actualYear1 + 1)).Select(z => z.id).FirstOrDefault();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            if (userRoles.Contains(
                db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                    .Select(r => r.id)
                    .FirstOrDefault()))
            {
                var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.academicyearId == AcademicYearId1 && c.e.IsCollegeEditable == false).Select(c => new
                {
                    collegeId = c.co.id,
                    collegeName = c.co.collegeCode + "-" + c.co.collegeName
                }).OrderBy(c => c.collegeName).ToList();

                colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

                ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            }
            else
            {
                int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Data Entry").Select(p => p.id).SingleOrDefault();
                int[] assignedcollegeslist =
                    db.jntuh_dataentry_allotment.Where(
                        d =>
                            d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
                            d.isCompleted == false).Select(s => s.collegeID).ToArray();
                ViewBag.Colleges =
                    db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                        (co, e) => new { co = co, e = e })
                        .Where(c =>c.e.academicyearId == AcademicYearId1 && c.e.IsCollegeEditable == false && assignedcollegeslist.Contains(c.e.collegeId))
                        .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                        .OrderBy(c => c.collegeName)
                        .ToList();
            }


           

            int SpecializationId = 0;
            TempData["UploadedCount"] = null;
            TempData["NotUploadedCount"] = null;
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegesIntakeWithLabs> intakedetailsList = new List<CollegesIntakeWithLabs>();
            List<Lab> labs = new List<Lab>();
           
            if(collegeId != null)
            {
                TempData["Collegeid"] = collegeId;
         
                int[] collegeIDs = null;

                if (collegeId != 0)
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                }
                else
                {
                  //  collegeIDs = db.jntuh_college.Where(c => AppealCollegeIds.Contains(c.id) && c.isActive == true).Select(c => c.id).ToArray();
                }

                List<int> collegeSpecializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == 9).Select(e => e.specializationId).Distinct().ToList();
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
                int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
                if (DegreeIds.Contains(4))
                    SpecializationId = 39;
                else
                    SpecializationId = 0;

                if (CollegeAffiliationStatus == "Yes")
                {
                    List<Lab> labsuploaded = new List<Lab>();
                    List<Lab> labsnotuploaded = new List<Lab>();



                    labsuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                    join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                    where lm.CollegeId == collegeId && Equipmentsids.Contains(lm.id)
                                    //(collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) &&
                                    select new Lab
                                    {
                                        id = l.id,
                                        collegeId = l.CollegeID,
                                        EquipmentNo = l.EquipmentNo,
                                        EquipmentID = lm.id,
                                        tempEquipmentId = l.EquipmentID,
                                        degree = lm.jntuh_degree.degree,
                                        degreeId = lm.DegreeID,
                                        department = lm.jntuh_department.departmentName,
                                        specializationName = lm.jntuh_specialization.specializationName,
                                        Semester = lm.Semester,
                                        year = lm.Year,
                                        Labcode = lm.Labcode,
                                        LabName = lm.LabName,
                                        AvailableArea = l.AvailableArea,
                                        RoomNumber = l.RoomNumber,
                                        EquipmentName = lm.EquipmentName ?? l.EquipmentName,
                                        Make = l.Make,
                                        Model = l.Model,
                                        EquipmentUniqueID = l.EquipmentUniqueID,
                                        AvailableUnits = l.AvailableUnits,
                                        specializationId = lm.SpecializationID,
                                        CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                        isCircuit = lm.jntuh_department.CircuitType,
                                        DisplayOrder = lm.jntuh_department.DisplayOrder,
                                        degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                        ViewEquipmentPhoto = l.EquipmentPhoto,
                                        EquipmentDateOfPurchasing = l.EquipmentDateOfPurchasing,
                                        DelivaryChalanaDate = l.DelivaryChalanaDate,
                                        updatedOn = l.updatedOn,
                                        updatedBy = l.updatedBy,
                                        createdOn = l.createdOn,
                                        createdBy = l.createdBy
                                    }).ToList();

                    labsnotuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                      // join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID 
                                       where lm.CollegeId == collegeId && !Equipmentsids.Contains(lm.id)
                                       //(collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) && lm.CollegeId == collegeId  
                                       select new Lab
                                       {
                                         //  id = l.id,
                                          // collegeId = lm.CollegeId,
                                          // EquipmentNo = l.EquipmentNo,
                                           EquipmentID = lm.id,
                                           tempEquipmentId = 0,
                                           degree = lm.jntuh_degree.degree,
                                           degreeId = lm.DegreeID,
                                           department = lm.jntuh_department.departmentName,
                                           specializationName = lm.jntuh_specialization.specializationName,
                                           Semester = lm.Semester,
                                           year = lm.Year,
                                           Labcode = lm.Labcode,
                                           LabName = lm.LabName,
                                           AvailableArea = 0,
                                           RoomNumber = string.Empty,
                                           EquipmentName = lm.EquipmentName ?? "",
                                           Make = string.Empty,
                                           Model = string.Empty,
                                           EquipmentUniqueID = string.Empty,
                                           AvailableUnits = 0,
                                           specializationId = lm.SpecializationID,
                                           CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                           isCircuit = lm.jntuh_department.CircuitType,
                                           DisplayOrder = lm.jntuh_department.DisplayOrder,
                                           degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                           ViewEquipmentPhoto = string.Empty,
                                           EquipmentDateOfPurchasing = null,
                                           DelivaryChalanaDate = null,
                                           updatedOn = null,
                                           updatedBy = null,
                                           createdOn = null,
                                           createdBy = null
                                       }).ToList();

                    TempData["UploadedCount"] = labsuploaded.Count();

                    labs.AddRange(labsuploaded);
                    labs.AddRange(labsnotuploaded);




                }
                else
                {

                    List<Lab> labsuploaded = new List<Lab>();
                    List<Lab> labsnotuploaded = new List<Lab>();

                    labsuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                    join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                    where (Equipmentsids.Contains(lm.id)  && l.CollegeID == collegeId && lm.CollegeId == null) && (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId)
                                    select new Lab
                                    {
                                        id = l.id,
                                        collegeId = l.CollegeID,
                                        EquipmentNo = l.EquipmentNo,
                                        EquipmentID = lm.id,
                                        tempEquipmentId = l.EquipmentID,
                                        degree = lm.jntuh_degree.degree,
                                        degreeId = lm.DegreeID,
                                        department = lm.jntuh_department.departmentName,
                                        specializationName = lm.jntuh_specialization.specializationName,
                                        Semester = lm.Semester,
                                        year = lm.Year,
                                        Labcode = lm.Labcode,
                                        LabName = lm.LabName,
                                        AvailableArea = l.AvailableArea,
                                        RoomNumber = l.RoomNumber,
                                        EquipmentName = lm.EquipmentName ?? l.EquipmentName,
                                        Make = l.Make,
                                        Model = l.Model,
                                        EquipmentUniqueID = l.EquipmentUniqueID,
                                        AvailableUnits = l.AvailableUnits,
                                        specializationId = lm.SpecializationID,
                                        CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                        isCircuit = lm.jntuh_department.CircuitType,
                                        DisplayOrder = lm.jntuh_department.DisplayOrder,
                                        degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                        ViewEquipmentPhoto = l.EquipmentPhoto,
                                        EquipmentDateOfPurchasing = l.EquipmentDateOfPurchasing,
                                        DelivaryChalanaDate = l.DelivaryChalanaDate,
                                        updatedOn =l.updatedOn,
                                        updatedBy =l.updatedBy,
                                        createdOn =l.createdOn,
                                        createdBy =l.createdBy
                                    }).ToList();

                    labsnotuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                       // join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                       where (!Equipmentsids.Contains(lm.id)  && lm.CollegeId == null) && (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId)
                                       select new Lab
                                       {
                                          // id = l.id,
                                         //  collegeId = l.CollegeID,
                                         //  EquipmentNo = l.EquipmentNo,
                                           EquipmentID = lm.id,
                                           tempEquipmentId = 0,//l.EquipmentID,
                                           degree = lm.jntuh_degree.degree,
                                           degreeId = lm.DegreeID,
                                           department = lm.jntuh_department.departmentName,
                                           specializationName = lm.jntuh_specialization.specializationName,
                                           Semester = lm.Semester,
                                           year = lm.Year,
                                           Labcode = lm.Labcode,
                                           LabName = lm.LabName,
                                           AvailableArea = 0,
                                           RoomNumber = string.Empty,
                                           EquipmentName = lm.EquipmentName ?? "",
                                           Make = string.Empty,
                                           Model = string.Empty,
                                           EquipmentUniqueID = string.Empty,
                                           AvailableUnits = 0,
                                           specializationId = lm.SpecializationID,
                                           CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                           isCircuit = lm.jntuh_department.CircuitType,
                                           DisplayOrder = lm.jntuh_department.DisplayOrder,
                                           degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                           ViewEquipmentPhoto = string.Empty,
                                           EquipmentDateOfPurchasing = null,
                                           DelivaryChalanaDate = null,
                                           updatedOn = null,
                                           updatedBy = null,
                                           createdOn = null,
                                           createdBy = null
                                       }).ToList();


                    labs.AddRange(labsuploaded);
                    labs.AddRange(labsnotuploaded);

                }

                labs = labs.OrderBy(e => e.department).ThenBy(e => e.degree).ThenBy(e => e.year).ThenBy(e => e.Semester).ThenBy(e => e.Labcode).ThenBy(e => e.LabName).ThenBy(e => e.EquipmentName).ToList();
                
                if(labs.Select(e=>e.specializationId).Contains(39))
                {
                    List<Lab> LabsCopy = labs.Where(e => e.specializationId == 39).Select(e => e).ToList();
                    labs.Where(e => e.specializationId == 39).ToList().ForEach(d => labs.Remove(d));
                    LabsCopy.ForEach(a => labs.Add(a));
                }
                
                
               return View("~/Views/Reports/CollegeIntakeWithLabs.cshtml", labs);
            }
            else
            {
                return View("~/Views/Reports/CollegeIntakeWithLabs.cshtml", labs);
            }
           // return View();
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult CollegeWiseNotUploadedLabs(int? collegeId)
        {
             int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if(userid != null)
            {
                var actualYear1 =
                  db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
              .Select(a => a.actualYear)
              .FirstOrDefault();
                var AcademicYearId1 =
                    db.jntuh_academic_year.Where(d => d.actualYear == (actualYear1 + 1)).Select(z => z.id).FirstOrDefault();

                var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, c => c.id, s => s.collegeId, (c, s) => new { s = s, c = c }).Where(co => co.c.isActive == true && co.s.academicyearId == AcademicYearId1 && co.s.IsCollegeEditable == false)
                    .Select(co => new
                    {
                        collegeId = co.c.id,
                        collegeName = co.c.collegeCode + "-" + co.c.collegeName
                    }).OrderBy(o=>o.collegeName).ToList();

                ViewBag.Colleges = colleges;
                List<Lab> Labs = new List<Lab>();
                if(collegeId != null)
                {
                    int SpecializationId = 0;

                    string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(c => c.collegeId == collegeId && c.affiliationTypeId == 7).Select(e => e.affiliationStatus).FirstOrDefault();
                    int[] collegeSpecializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == 9).Select(e => e.specializationId).Distinct().ToArray();
                    int[] Equipmentsids = db.jntuh_college_laboratories.Where(e => e.CollegeID == collegeId).Select(e => e.EquipmentID).ToArray();
                    int[] DegreeIds = db.jntuh_lab_master.Where(e => Equipmentsids.Contains(e.id)).Select(e => e.DegreeID).Distinct().ToArray();
                    if (DegreeIds.Contains(4))
                        SpecializationId = 39;
                    else
                        SpecializationId = 0;

                   // List<Lab> Labs = new List<Lab>();
                    if(CollegeAffiliationStatus == "Yes")
                    {
                        Labs = (from lm in db.jntuh_lab_master.AsNoTracking()
                                // join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID 
                                where lm.CollegeId == collegeId && !Equipmentsids.Contains(lm.id)
                                //(collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) && lm.CollegeId == collegeId  
                                select new Lab
                                {
                                    //  id = l.id,
                                    // collegeId = lm.CollegeId,
                                    // EquipmentNo = l.EquipmentNo,
                                    EquipmentID = lm.id,
                                    tempEquipmentId = 0,
                                    degree = lm.jntuh_degree.degree,
                                    degreeId = lm.DegreeID,
                                    department = lm.jntuh_department.departmentName,
                                    specializationName = lm.jntuh_specialization.specializationName,
                                    Semester = lm.Semester,
                                    year = lm.Year,
                                    Labcode = lm.Labcode,
                                    LabName = lm.LabName,
                                    AvailableArea = 0,
                                    RoomNumber = string.Empty,
                                    EquipmentName = lm.EquipmentName ?? "",
                                    Make = string.Empty,
                                    Model = string.Empty,
                                    EquipmentUniqueID = string.Empty,
                                    AvailableUnits = 0,
                                    specializationId = lm.SpecializationID,
                                    CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                    isCircuit = lm.jntuh_department.CircuitType,
                                    DisplayOrder = lm.jntuh_department.DisplayOrder,
                                    degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                    ViewEquipmentPhoto = string.Empty,
                                    EquipmentDateOfPurchasing = null,
                                    DelivaryChalanaDate = null,
                                    updatedOn = null,
                                    updatedBy = null,
                                    createdOn = null,
                                    createdBy = null
                                }).ToList();
                    }
                    else
                    {
                        Labs = (from lm in db.jntuh_lab_master.AsNoTracking()
                                           // join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                           where (!Equipmentsids.Contains(lm.id) && lm.CollegeId == null) && (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId)
                                           select new Lab
                                           {
                                               // id = l.id,
                                               //  collegeId = l.CollegeID,
                                               //  EquipmentNo = l.EquipmentNo,
                                               EquipmentID = lm.id,
                                               tempEquipmentId = 0,//l.EquipmentID,
                                               degree = lm.jntuh_degree.degree,
                                               degreeId = lm.DegreeID,
                                               department = lm.jntuh_department.departmentName,
                                               specializationName = lm.jntuh_specialization.specializationName,
                                               Semester = lm.Semester,
                                               year = lm.Year,
                                               Labcode = lm.Labcode,
                                               LabName = lm.LabName,
                                               AvailableArea = 0,
                                               RoomNumber = string.Empty,
                                               EquipmentName = lm.EquipmentName ?? "",
                                               Make = string.Empty,
                                               Model = string.Empty,
                                               EquipmentUniqueID = string.Empty,
                                               AvailableUnits = 0,
                                               specializationId = lm.SpecializationID,
                                               CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                               isCircuit = lm.jntuh_department.CircuitType,
                                               DisplayOrder = lm.jntuh_department.DisplayOrder,
                                               degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                               ViewEquipmentPhoto = string.Empty,
                                               EquipmentDateOfPurchasing = null,
                                               DelivaryChalanaDate = null,
                                               updatedOn = null,
                                               updatedBy = null,
                                               createdOn = null,
                                               createdBy = null
                                           }).ToList();
                    }

                    Labs = Labs.OrderBy(e => e.department).ThenBy(e => e.degree).ThenBy(e => e.year).ThenBy(e => e.Semester).ToList();

                    if (Labs.Select(e => e.specializationId).Contains(39))
                    {
                        List<Lab> LabsCopy = Labs.Where(e => e.specializationId == 39).Select(e => e).ToList();
                        Labs.Where(e => e.specializationId == 39).ToList().ForEach(d => Labs.Remove(d));
                        LabsCopy.ForEach(a => Labs.Add(a));
                    }
                    ViewBag.NotUploadCount = Labs.Count();
                    return View(Labs);
                }
                else
                {
                    return View(Labs);
                }
            }
            else
            {
                return RedirectToAction("LogOn","Account");
            }
        }
         

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AllCollegesIntakeWithLabsDetails(int specializationId, int year, int semester, int collegeId, string type, int DegreeIds)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);
            string strdegree = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.jntuh_degree.degree).FirstOrDefault();
            string strdepartment = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.departmentName).FirstOrDefault();

            //string strcollegecode= db.jntuh_college_randamcodes.Find(collegeId).RandamCode;

            //jntuh_college_randamcodes randamCodes = new Models.jntuh_college_randamcodes();
            // randamCodes.RandamCode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault(); ;

            //if (strdegree != "B.Tech" && strdegree != "B.Pharmacy")
            //{
            //    year = 0;
            //    semester = 0;
            //}



             //int SpecializationId = 0;
          //  int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
           // int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
            //if (DegreeIds.Contains(4))
             //   SpecializationId = 39;
           // else
                //SpecializationId = 0;


            //int DegreeId = db.jntuh_lab_master.Where(M => M.SpecializationID == specializationId).Select(M=>M.DegreeID).FirstOrDefault();

            int DegreeId = DegreeIds;
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            List<jntuh_lab_master> collegeLabMaster = null;
            if (CollegeAffiliationStatus == "Yes")
            {
                if (DegreeId == 1)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 1 && l.CollegeId == collegeId).ToList(); // && l.Year == year && l.Semester == semester
                else if (DegreeId == 2)
                {
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.CollegeId == collegeId).ToList(); //  && l.Year == year && l.Semester == semester
                }

                else if (DegreeId == 3)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 3 && l.CollegeId == collegeId).ToList();
                else if (DegreeId == 4)
                {
                    if (year == 1 && semester == 1)
                    {
                        //collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l =>l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.CollegeId == collegeId && l.DegreeID == 4 || l.SpecializationID == 39).ToList();
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.CollegeId == collegeId && l.DegreeID == 4 && l.SpecializationID == 39).ToList();
                    }
                    else
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.CollegeId == collegeId).ToList();
                    }
                }
                else if (DegreeId == 5)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 5 && l.CollegeId == collegeId).ToList();  //  && l.Year == year && l.Semester == semester
                else if (DegreeId == 6)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 6 && l.CollegeId == collegeId).ToList();
                else if (DegreeId == 9)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 9 && l.CollegeId == collegeId).ToList();
                else if (DegreeId == 10)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 10 && l.CollegeId == collegeId).ToList();
            }
            else if ((CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null))
            {
                if (DegreeId == 1)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 1 && l.Year == year && l.Semester == semester && l.CollegeId ==null).ToList(); //   && l.Year == year && l.Semester == semester 
                else if (DegreeId == 2)
                {
                    if (year == 1 && semester == 1)
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == year && l.Semester == semester && l.CollegeId == null || (l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == 0 && l.Semester == 0 && l.Labcode != "TMP-CL")).ToList();  // && l.Year == year && l.Semester == semester
                    else
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == year && l.Semester == semester && l.CollegeId == null).ToList();  // && l.Year == year && l.Semester == semester
                }

                else if (DegreeId == 3)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 3 && l.Year == year && l.Semester == semester && l.CollegeId == null).ToList();
                else if (DegreeId == 4)
                {
                    if (year == 1 && semester == 1)
                    {
                        //collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.CollegeId == null || l.SpecializationID == 39).ToList();
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == 39 && l.DegreeID == 4 && l.CollegeId == null).ToList();
                    }
                    else
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.CollegeId == null).ToList();
                    }
                }

                else if (DegreeId == 5)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.CollegeId == null && l.DegreeID == 5).ToList();  //  l.Semester == semester && l.DegreeID == 5 && && l.Year == year
                else if (DegreeId == 6)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.CollegeId == null && l.DegreeID == 6).ToList();
                else if (DegreeId == 9)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.CollegeId == null && l.DegreeID == 9).ToList();
                else if (DegreeId == 10)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.CollegeId == null && l.DegreeID == 10).ToList();
            }






            #region Committed by Suresh

            //if (DegreeId == 1)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 1  && l.Labcode != "TMP-CL").ToList(); //   && l.Year == year && l.Semester == semester 
            //else if (DegreeId == 2)
            //{
            //    if (year == 1 && semester == 1)
            //        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == year && l.Semester == semester && l.Labcode != "TMP-CL" || (l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == 0 && l.Semester == 0 && l.Labcode != "TMP-CL")).ToList();  // && l.Year == year && l.Semester == semester
            //    else
            //        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == year && l.Semester == semester && l.Labcode != "TMP-CL").ToList();  // && l.Year == year && l.Semester == semester
            //}

            //else if (DegreeId == 3)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 3 && l.Year == year && l.Semester == semester && l.Labcode != "TMP-CL").ToList();
            //else if (DegreeId == 4)
            //{

            //    if ((year == 1 && (semester == 1 || specializationId == 39)) || (year == 3 && semester == 0))
            //    {
            //        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == 4 && l.Labcode != "TMP-CL" && l.SpecializationID == 39).ToList();   //l.SpecializationID == specializationId &&  && l.Semester == semester
            //    }
            //    else
            //    {
            //        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.Labcode != "TMP-CL").ToList();
                    
            //    }
            //}

            //else if (DegreeId == 5)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Labcode != "TMP-CL" && l.DegreeID == 5).ToList();  //  l.Semester == semester && l.DegreeID == 5 && && l.Year == year
            //else if (DegreeId == 6)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Labcode != "TMP-CL" && l.DegreeID == 6).ToList();
            //else if (DegreeId == 9)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Labcode != "TMP-CL" && l.DegreeID == 9).ToList();
            //else if (DegreeId == 10)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Labcode != "TMP-CL" && l.DegreeID == 10).ToList();

            #endregion

            var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();
           

            #region Old Code Commented By Srinivas
            //foreach (var item in collegeLabMaster)
            //{
            //    if (item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy")
            //    {
            //        for (int i = 1; i <= PGEquipmentCount; i++)
            //        {
            //            Lab lstlabs = new Lab();
            //            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
            //            lstlabs.EquipmentID = item.id;
            //            lstlabs.degree = item.jntuh_degree.degree;
            //            lstlabs.department = item.jntuh_department.departmentName;
            //            lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //            lstlabs.Semester = item.Semester;
            //            lstlabs.year = item.Year;
            //            lstlabs.Labcode = item.Labcode;
            //            lstlabs.LabName = item.LabName;
            //            lstlabs.EquipmentName = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
            //            lstlabs.LabEquipmentName = item.EquipmentName;
            //            lstlabs.collegeId = collegeId;
            //            lstlabs.EquipmentNo = i;
            //            lstlabs.RandomCode = strcollegecode;
            //            lstlaboratories.Add(lstlabs);
            //        }
            //    }
            //    else
            //    {
            //        Lab lstlabs = new Lab();
            //        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
            //        lstlabs.EquipmentID = item.id;
            //        lstlabs.degree = item.jntuh_degree.degree;
            //        lstlabs.department = item.jntuh_department.departmentName;
            //        lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //        lstlabs.Semester = item.Semester;
            //        lstlabs.year = item.Year;
            //        lstlabs.Labcode = item.Labcode;
            //        lstlabs.LabName = item.LabName;
            //        lstlabs.EquipmentName = item.EquipmentName;
            //        lstlabs.LabEquipmentName = item.EquipmentName;
            //        lstlabs.collegeId = collegeId;
            //        lstlabs.EquipmentNo = 1;
            //        lstlabs.RandomCode = strcollegecode;
            //        lstlaboratories.Add(lstlabs);
            //    }
            //}
            #endregion

            #region New Code Written By Srinivas
            if (collegeLabMaster!=null)
            {
                foreach (var item in collegeLabMaster)
                {

                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                    lstlabs.EquipmentID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName = !string.IsNullOrEmpty(item.LabName) ? item.LabName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.LabName).FirstOrDefault();
                    lstlabs.EquipmentName = !string.IsNullOrEmpty(item.EquipmentName) ? item.EquipmentName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
                    lstlabs.LabEquipmentName = !string.IsNullOrEmpty(item.EquipmentName) ? item.EquipmentName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault(); 
                    lstlabs.collegeId = collegeId;
                    lstlabs.EquipmentNo = 1;
                    lstlabs.EquipmentDateOfPurchasing =
                        jntuh_college_laboratories.Where(L => L.EquipmentID == item.id && L.CollegeID == collegeId)
                            .Select(L => L.EquipmentDateOfPurchasing)
                            .FirstOrDefault();
                    lstlabs.RandomCode = strcollegecode;
                    lstlabs.updatedBy = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id  && l.CollegeID == collegeId).Select(l => l.updatedBy).FirstOrDefault();
                    lstlabs.updatedOn = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id  && l.CollegeID == collegeId).Select(l => l.updatedOn).FirstOrDefault();
                    lstlabs.createdOn = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.createdOn).FirstOrDefault();
                    lstlaboratories.Add(lstlabs);





                }
            }
           
            #endregion

           
            if (type == "Excel")
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + strcollegecode + "-" + strdegree + "-" + strdepartment + " Labs.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_AllCollegesIntakeWithLabsDetailsExportToExcel.cshtml", lstlaboratories);
            }
            return View("~/Views/Reports/AllCollegesIntakeWithLabsDetails.cshtml", lstlaboratories);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AutonomusCollegesIntakeWithLabsDetails(int specializationId, int year, int semester, int collegeId, string type, int DegreeIds)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);
            string strdegree = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.jntuh_degree.degree).FirstOrDefault();
            string strdepartment = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.departmentName).FirstOrDefault();

            //string strcollegecode= db.jntuh_college_randamcodes.Find(collegeId).RandamCode;

            //jntuh_college_randamcodes randamCodes = new Models.jntuh_college_randamcodes();
            // randamCodes.RandamCode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault(); ;

            //if (strdegree != "B.Tech" && strdegree != "B.Pharmacy")
            //{
            //    year = 0;
            //    semester = 0;
            //}



            //int SpecializationId = 0;
            //  int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
            // int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
            //if (DegreeIds.Contains(4))
            //   SpecializationId = 39;
            // else
            //SpecializationId = 0;
            int DegreeId = DegreeIds;
            //int DegreeId = db.jntuh_lab_master.Where(M => M.SpecializationID == specializationId).Select(M => M.DegreeID).FirstOrDefault();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            List<jntuh_lab_master> collegeLabMaster = null;
            if (CollegeAffiliationStatus == "Yes")
            {
                if (DegreeId == 1)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 1 && l.Labcode == "TMP-CL").ToList(); // && l.Year == year && l.Semester == semester
                else if (DegreeId == 2)
                {
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Labcode == "TMP-CL").ToList(); //  && l.Year == year && l.Semester == semester
                }

                else if (DegreeId == 3)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 3 && l.Labcode == "TMP-CL").ToList();
                else if (DegreeId == 4)
                {
                    if (year == 1 && semester == 1)
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.Year == 0 && l.Semester == 0 && l.DegreeID == 4 && l.Labcode == "TMP-CL" && l.SpecializationID == specializationId).ToList();
                    }
                    else
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.Labcode == "TMP-CL").ToList();
                    }
                }
                else if (DegreeId == 5)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 5 && l.Labcode == "TMP-CL").ToList();  //  && l.Year == year && l.Semester == semester
                else if (DegreeId == 6)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 6 && l.Labcode == "TMP-CL").ToList();
                else if (DegreeId == 9)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 9 && l.Labcode == "TMP-CL").ToList();
                else if (DegreeId == 10)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 10 && l.Labcode == "TMP-CL").ToList();
            }
           


            var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();

            #region New Code Written By Srinivas
            if (collegeLabMaster!=null)
            {
                foreach (var item in collegeLabMaster)
                {

                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                    lstlabs.EquipmentID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName =!string.IsNullOrEmpty(item.LabName)?item.LabName: jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.LabName).FirstOrDefault(); 
                    lstlabs.EquipmentName =!string.IsNullOrEmpty(item.EquipmentName)?item.EquipmentName: jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
                    lstlabs.LabEquipmentName = !string.IsNullOrEmpty(item.EquipmentName)? item.EquipmentName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault(); 
                    lstlabs.collegeId = collegeId;
                    lstlabs.EquipmentNo = 1;
                    lstlabs.RandomCode = strcollegecode;
                    lstlabs.updatedBy = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.updatedBy).FirstOrDefault();
                    lstlabs.updatedOn = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.updatedOn).FirstOrDefault();
                    lstlabs.createdOn = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.createdOn).FirstOrDefault();
                    lstlaboratories.Add(lstlabs);
                }
            }
            #endregion


            if (type == "Excel")
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + strcollegecode + "-" + strdegree + "-" + strdepartment + " Labs.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_AllCollegesIntakeWithLabsDetailsExportToExcel.cshtml", lstlaboratories);
            }
            return View("~/Views/Reports/AutonomusCollegesIntakeWithLabsDetails.cshtml", lstlaboratories);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AllCollegesLabsDetails(string type, int? cid)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

            int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();//.Take(1).ToArray();  && c.id == cid

            foreach (var collegeId in collegeIDs)
            {
                string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault();
                int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e.specializationId).Distinct().ToArray();

                List<jntuh_lab_master> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                            .Where(l => specializationIds.Contains(l.SpecializationID))
                                                            .OrderBy(l => l.jntuh_degree.degreeDisplayOrder)
                                                            .ThenBy(l => l.jntuh_department.departmentName)
                                                            .ThenBy(l => l.jntuh_specialization.specializationName)
                                                            .ThenBy(l => l.Year).ThenBy(l => l.Semester)
                                                            .ToList();

                var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();

                foreach (var item in collegeLabMaster)
                {
                    if (item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy")
                    {
                        for (int i = 1; i <= PGEquipmentCount; i++)
                        {
                            Lab lstlabs = new Lab();
                            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                            lstlabs.EquipmentID = item.id;
                            lstlabs.degree = item.jntuh_degree.degree;
                            lstlabs.department = item.jntuh_department.departmentName;
                            lstlabs.specializationName = item.jntuh_specialization.specializationName;
                            lstlabs.Semester = item.Semester;
                            lstlabs.year = item.Year;
                            lstlabs.Labcode = item.Labcode;
                            lstlabs.LabName = item.LabName;
                            lstlabs.EquipmentName = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
                            lstlabs.LabEquipmentName = item.EquipmentName;
                            lstlabs.collegeId = collegeId;
                            lstlabs.EquipmentNo = i;
                            lstlabs.RandomCode = strcollegecode;
                            lstlaboratories.Add(lstlabs);
                        }
                    }
                    else
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                        lstlabs.EquipmentID = item.id;
                        lstlabs.degree = item.jntuh_degree.degree;
                        lstlabs.department = item.jntuh_department.departmentName;
                        lstlabs.specializationName = item.jntuh_specialization.specializationName;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.Year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.LabName = item.LabName;
                        lstlabs.EquipmentName = item.EquipmentName;
                        lstlabs.LabEquipmentName = item.EquipmentName;
                        lstlabs.collegeId = collegeId;
                        lstlabs.EquipmentNo = 1;
                        lstlabs.RandomCode = strcollegecode;
                        lstlaboratories.Add(lstlabs);
                    }
                }
            }

            if (type == "Excel")
            {
                string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == cid).Select(r => r.RandamCode).FirstOrDefault();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename= " + strcollegecode + "-Labs.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_AllCollegesIntakeWithLabsDetailsExportToExcel.cshtml", lstlaboratories);
            }

            return View("~/Views/Reports/AllCollegesIntakeWithLabsDetails.cshtml", lstlaboratories);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult CollegeWiseLabs(string type, int? collegeId)
        {
            if (collegeId != null)
            {
                DataSet ds = new DataSet();

                int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

                string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault();
                int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e.specializationId).Distinct().ToArray();

                var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();

                foreach (var specializationId in specializationIds)
                {
                    string strdegree = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.jntuh_degree.degree).FirstOrDefault();
                    string strdepartment = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.departmentName).FirstOrDefault();
                    string strspecialization = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.specializationName).FirstOrDefault();

                    for (int year = 0; year <= 4; year++)
                    {
                        if (year != 1)
                        {
                            for (int semister = 0; semister <= 2; semister++)
                            {
                                List<Lab> lstlaboratories = new List<Lab>();
                                List<jntuh_lab_master> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                                            .Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semister)
                                                                            .ToList();

                                foreach (var item in collegeLabMaster)
                                {
                                    if (item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy")
                                    {
                                        for (int i = 1; i <= PGEquipmentCount; i++)
                                        {
                                            Lab lstlabs = new Lab();
                                            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                                            lstlabs.EquipmentID = item.id;
                                            lstlabs.degree = item.jntuh_degree.degree;
                                            lstlabs.department = item.jntuh_department.departmentName;
                                            lstlabs.specializationName = item.jntuh_specialization.specializationName;
                                            lstlabs.Semester = item.Semester;
                                            lstlabs.year = item.Year;
                                            lstlabs.Labcode = item.Labcode;
                                            lstlabs.LabName = item.LabName;
                                            lstlabs.EquipmentName = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
                                            lstlabs.LabEquipmentName = item.EquipmentName;
                                            lstlabs.collegeId = (int)collegeId;
                                            lstlabs.EquipmentNo = i;
                                            lstlabs.RandomCode = strcollegecode;
                                            lstlaboratories.Add(lstlabs);
                                        }
                                    }
                                    else
                                    {
                                        Lab lstlabs = new Lab();
                                        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                                        lstlabs.EquipmentID = item.id;
                                        lstlabs.degree = item.jntuh_degree.degree;
                                        lstlabs.department = item.jntuh_department.departmentName;
                                        lstlabs.specializationName = item.jntuh_specialization.specializationName;
                                        lstlabs.Semester = item.Semester;
                                        lstlabs.year = item.Year;
                                        lstlabs.Labcode = item.Labcode;
                                        lstlabs.LabName = item.LabName;
                                        lstlabs.EquipmentName = item.EquipmentName;
                                        lstlabs.LabEquipmentName = item.EquipmentName;
                                        lstlabs.collegeId = (int)collegeId;
                                        lstlabs.EquipmentNo = 1;
                                        lstlabs.RandomCode = strcollegecode;
                                        lstlaboratories.Add(lstlabs);
                                    }
                                }

                                if (lstlaboratories.Count() > 0)
                                {
                                    // \/?*[]
                                    strspecialization = strspecialization.Replace("'\'", "");
                                    strspecialization = strspecialization.Replace("/", "");
                                    strspecialization = strspecialization.Replace("?", "");
                                    strspecialization = strspecialization.Replace("[", "");
                                    strspecialization = strspecialization.Replace("]", "");

                                    string sFileName = strdegree + "-" + strspecialization.Substring(0, strspecialization.Length > 7 ? 7 : strspecialization.Length) + "-" + year + "-" + semister;
                                    if (strdegree != "B.Tech" && strdegree != "B.Pharmacy")
                                    {
                                        sFileName = strdegree + "-" + strspecialization.Substring(0, strspecialization.Length > 19 ? 19 : strspecialization.Length);
                                        //sFileName = strdegree + "-" + strspecialization;
                                    }

                                    var lst = lstlaboratories.Select((l, index) => new
                                    {
                                        SNo = index + 1,
                                        CC = l.RandomCode,
                                        Degree = l.degree,
                                        Department = l.department,
                                        Specialization = l.specializationName,
                                        Year = l.year == 0 ? (int?)null : l.year,
                                        Semister = l.Semester == 0 ? (int?)null : l.Semester,
                                        LabCode = l.Labcode,
                                        LabName = l.LabName,
                                        EquipmentName = l.EquipmentName,
                                        Available = l.id == 0 ? "NO" : "YES",
                                    }).ToList();

                                    ds.Tables.Add(ToDataTable(lst, sFileName));
                                }

                            }
                        }

                    }
                }

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(ds);
                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wb.Style.Font.Bold = true;
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename= " + strcollegecode + "-Labs.xlsx");
                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        wb.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }

                List<Lab> lstlaboratories1 = new List<Lab>();

                return View("~/Views/Reports/AllCollegesIntakeWithLabsDetails.cshtml", lstlaboratories1);
            }
            else
            {
                return RedirectToAction("AllCollegesIntakeWithLabs");
            }
        }

        public DataTable ToDataTable<T>(IList<T> data, string tableName)// T is any generic type
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            table.TableName = tableName;
            return table;
        }

        public void Export(List<Lab> lstlaboratories, string code, string degree, string department, string specialization, int year, int semister)
        {
            StringBuilder sb = new StringBuilder();
            string sFileName = code + "-" + degree + "-" + department + "-" + specialization + "-" + year + "-" + semister + ".xls";

            var Data = lstlaboratories;

            if (Data != null && Data.Any())
            {
                sb.Append("<table border='1' style='width: 100%;'>");
                sb.Append("<tr style='font-weight: bold;'>");
                sb.Append("<td style='text-align: center;background-color:yellow;vertical-align:top;'>S.No</td>");
                sb.Append("<td style='text-align: center;background-color:yellow;vertical-align:top;'>CC</td>");
                sb.Append("<td style='text-align: left; background-color:yellow;vertical-align:top;'>Degree</td>");
                sb.Append("<td style='text-align: left; background-color:yellow;vertical-align:top;'>Department</td>");
                sb.Append("<td style='text-align: left;background-color:yellow;vertical-align:top;'>Specialization</td>");
                sb.Append("<td style='text-align: center;background-color:yellow;vertical-align:top;'>year</td>");
                sb.Append("<td style='text-align: center;background-color:yellow;vertical-align:top;'>Semester</td>");
                sb.Append("<td style='text-align: center;background-color:yellow;vertical-align:top;'>Lab Code</td>");
                sb.Append("<td style='text-align: left;background-color:yellow;vertical-align:top;'>Lab Name</td>");
                sb.Append("<td style='text-align: left;background-color:yellow;vertical-align:top;'>Equipment Name</td>");
                sb.Append("<td style='text-align: center;background-color:yellow;vertical-align:top;'>Available</td>");
                sb.Append("</tr>");

                foreach (var item in Data)
                {
                    sb.Append("<tr>");
                    sb.Append("<td style='text-align: center;vertical-align:top;'>" + (Data.IndexOf(item) + 1) + "</td>");
                    sb.Append("<td style='text-align: center;vertical-align:top;'>" + item.RandomCode + "</td>");
                    sb.Append("<td style='text-align: left;vertical-align:top;'>" + item.degree + "</td>");
                    sb.Append("<td style='text-align: left;vertical-align:top;'>" + item.department + "</td>");
                    sb.Append("<td style='text-align: left;vertical-align:top;'>" + item.specializationName + "</td>");
                    if (item.year != 0)
                    {
                        sb.Append("<td style='text-align: center;vertical-align:top;'>" + item.year + "</td>");
                    }
                    else
                    {
                        sb.Append("<td></td>");
                    }

                    if (item.Semester != 0)
                    {
                        sb.Append("<td style='text-align: center;vertical-align:top;'>" + item.Semester + "</td>");
                    }
                    else
                    {
                        sb.Append("<td></td>");
                    }

                    sb.Append("<td style='text-align: center;vertical-align:top;'>" + item.Labcode + "</td>");
                    sb.Append("<td style='text-align: left;vertical-align:top;'>" + item.LabName + "</td>");
                    sb.Append("<td style='text-align: left;vertical-align:top;'>" + item.EquipmentName + "</td>");

                    var eqid = item.EquipmentID;
                    var id = item.id;
                    var EquipmentNo = item.EquipmentNo;
                    var collegeId = item.collegeId;
                    int count = db.jntuh_college_laboratories.Where(l => l.id == id && l.EquipmentNo == EquipmentNo && l.CollegeID == collegeId).Select(l => l.id).Count();

                    if (count > 0)
                    {
                        sb.Append("<td style='background-color: green;text-align:center'>Yes</td>");
                    }
                    else
                    {
                        sb.Append("<td style='background-color: red;text-align:center'>No</td>");
                    }
                    sb.Append("</tr>");
                }
            }

            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.AddHeader("content-disposition", "attachment; filename=" + sFileName);
            Response.Charset = "";
            Response.Cache.SetNoServerCaching();
            Response.ContentType = "application/vnd.ms-excel";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            Response.BinaryWrite(buffer);
            Response.Flush();
            Response.End();
            Response.Close();
            Response.ExpiresAbsolute = DateTime.Now;

            //return File(buffer, "application/vnd.ms-excel");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, int collegeId, int? eqpid, int? eqpno)
        {

         
           Lab laboratories = new Lab();
           // List<Lab> laboratories = new List<Lab>();
            laboratories.collegeId = collegeId;


            if (id != null)
            {
                ViewBag.IsUpdate = true;
                laboratories = (from m in db.jntuh_lab_master
                                join labs in db.jntuh_college_laboratories on m.id equals labs.EquipmentID
                                where (labs.CollegeID == collegeId && labs.id == id)
                                select new Lab
                                {
                                    id = labs.id,
                                    collegeId = collegeId,
                                    EquipmentID = labs.EquipmentID,
                                    EquipmentName = !string.IsNullOrEmpty(m.EquipmentName)?m.EquipmentName:labs.EquipmentName,
                                    LabEquipmentName = !string.IsNullOrEmpty(labs.EquipmentName)?labs.EquipmentName:m.EquipmentName,
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
                                    LabName = !string.IsNullOrEmpty(labs.LabName)?labs.LabName:m.LabName,
                                    EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                    DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                    ViewEquipmentPhoto = labs.EquipmentPhoto,
                                  //  EquipmentPhoto = labs.EquipmentPhoto,
                                    NoPIDoc = labs.NOPIDOC != false ? true : false,
                                    NoDCDoc = labs.NODCdoc != false ? true : false,
                                }).FirstOrDefault();
                laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                TempData["DegreeId"] = laboratories.degreeId;
                laboratories.DelivaryChalanaDate1 = laboratories.DelivaryChalanaDate != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                return PartialView("~/Views/Reports/_AllCollegesIntakeWithLabsDetails.cshtml", laboratories);
              //  return PartialView("~/Views/Reports/CollegeIntakeWithLabs.cshtml", laboratories);
            }
            else
            {
                ViewBag.IsUpdate = false;
                jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                laboratories.collegeId = collegeId;
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
               
                laboratories.EquipmentID = master.id;
                laboratories.Semester = master.Semester;
                laboratories.Labcode = master.Labcode;
                TempData["DegreeId"] = laboratories.degreeId;
              return PartialView("~/Views/Reports/_AllCollegesIntakeWithLabsDetails.cshtml", laboratories);
               // return PartialView("~/Views/Reports/CollegeIntakeWithLabs.cshtml", laboratories);
            }

        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College,DataEntry")]
        public ActionResult AddEditRecord(Lab laboratories, string cmd)
        {
            //int userID =1;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (laboratories.collegeId == 0)
            {
                if (TempData["Collegeid"] != null)
                {
                    laboratories.collegeId = Convert.ToInt32(TempData["Collegeid"]);
                }
            }
            
            

            if (laboratories.EquipmentUniqueID == null)
            {
                laboratories.EquipmentUniqueID = string.Empty;
            }
            if (ModelState.IsValid)
            {
                jntuh_college_laboratories jntuh_college_laboratories = new jntuh_college_laboratories();
                jntuh_college_laboratories.CollegeID = laboratories.collegeId;
                jntuh_college_laboratories.EquipmentID = laboratories.EquipmentID;
                jntuh_college_laboratories.Make = laboratories.Make;
                jntuh_college_laboratories.Model = laboratories.Model;
                jntuh_college_laboratories.EquipmentUniqueID = laboratories.EquipmentUniqueID;
                jntuh_college_laboratories.EquipmentName = laboratories.EquipmentName;
                jntuh_college_laboratories.AvailableUnits = laboratories.AvailableUnits;
                jntuh_college_laboratories.AvailableArea = laboratories.AvailableArea;
                jntuh_college_laboratories.RoomNumber = laboratories.RoomNumber;
                jntuh_college_laboratories.EquipmentNo = laboratories.EquipmentNo;
                jntuh_college_laboratories.isActive = true;
                jntuh_college_laboratories.LabName = laboratories.LabName;
                jntuh_college_laboratories.NOPIDOC = laboratories.NoPIDoc;
                jntuh_college_laboratories.NODCdoc = laboratories.NoDCDoc;
                jntuh_college_laboratories.EquipmentPhoto = laboratories.ViewEquipmentPhoto;
                if (laboratories.EquipmentDateOfPurchasing1 != null)
                {
                    laboratories.EquipmentDateOfPurchasing1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing1);
                    jntuh_college_laboratories.EquipmentDateOfPurchasing = Convert.ToDateTime(laboratories.EquipmentDateOfPurchasing1);

                }

                if (laboratories.DelivaryChalanaDate1 != null)
                {
                    laboratories.DelivaryChalanaDate1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate1);
                    jntuh_college_laboratories.DelivaryChalanaDate = Convert.ToDateTime(laboratories.DelivaryChalanaDate1);
                }






                if (cmd == "Save")
                {
                    var existingID = db.jntuh_college_laboratories.Where(c => c.CollegeID == laboratories.collegeId && c.EquipmentID == laboratories.EquipmentID && c.EquipmentNo == laboratories.EquipmentNo).Select(c => c).FirstOrDefault();

                    if (existingID == null)
                    {
                        jntuh_college_laboratories.createdBy = userID;
                        jntuh_college_laboratories.createdOn = DateTime.Now;
                        db.jntuh_college_laboratories.Add(jntuh_college_laboratories);
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
                    jntuh_college_laboratories.id = (int)laboratories.id;
                    jntuh_college_laboratories.createdBy = laboratories.createdBy;
                    jntuh_college_laboratories.createdOn = laboratories.createdOn;
                    jntuh_college_laboratories.updatedBy = userID;
                    jntuh_college_laboratories.updatedOn = DateTime.Now;
                    jntuh_college_laboratories.isActive = true;
                    db.Entry(jntuh_college_laboratories).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Lab Updated Successfully.";
                }
            }
            if (laboratories.specializationId == 39)
              //  return RedirectToAction("AllCollegesIntakeWithLabsDetails", new { specializationId = laboratories.specializationId, year = laboratories.year, semester =1, collegeId = laboratories.collegeId, DegreeIds = TempData["DegreeId"] });
                return RedirectToAction("AllCollegesIntakeWithLabsDetailsNew", new { collegeId = laboratories.collegeId });
            else
          //  return RedirectToAction("AllCollegesIntakeWithLabsDetails", new { specializationId = laboratories.specializationId, year = laboratories.year, semester = laboratories.Semester, collegeId = laboratories.collegeId, DegreeIds = TempData["DegreeId"] });
                return RedirectToAction("AllCollegesIntakeWithLabsDetailsNew", new { collegeId = laboratories.collegeId });
        }



        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College,DataEntry")]
        public ActionResult AutonomousAddEditRecord(int? id, int collegeId, int? eqpid, int? eqpno)
        {


            Lab laboratories = new Lab();
            laboratories.collegeId = collegeId;


            if (id != null)
            {
                ViewBag.IsUpdate = true;
                laboratories = (from m in db.jntuh_lab_master
                                join labs in db.jntuh_college_laboratories on m.id equals labs.EquipmentID
                                where (labs.CollegeID == collegeId && labs.id == id)
                                select new Lab
                                {
                                    id = labs.id,
                                    collegeId = collegeId,
                                    EquipmentID = labs.EquipmentID,
                                    EquipmentName = !string.IsNullOrEmpty(m.EquipmentName) ? m.EquipmentName : labs.EquipmentName,
                                    LabEquipmentName = !string.IsNullOrEmpty(labs.EquipmentName) ? labs.EquipmentName : m.EquipmentName,
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
                                    LabName = !string.IsNullOrEmpty(labs.LabName) ? labs.LabName : m.LabName,
                                    EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                    DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                    ViewEquipmentPhoto = labs.EquipmentPhoto,
                                    NoPIDoc = labs.NOPIDOC != false ? true : false,
                                    NoDCDoc = labs.NODCdoc != false ? true : false
                                }).FirstOrDefault();
                laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                TempData["DegreeId"] = laboratories.degreeId;
                laboratories.DelivaryChalanaDate1 = laboratories.DelivaryChalanaDate != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                return PartialView("~/Views/Reports/_AutonomousCollegesIntakeWithLabsDetails.cshtml", laboratories);
            }
            else
            {
                ViewBag.IsUpdate = false;
                jntuh_lab_master master = db.jntuh_lab_master.Find(eqpid);
                laboratories.collegeId = collegeId;
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

                laboratories.EquipmentID = master.id;
                laboratories.Semester = master.Semester;
                laboratories.Labcode = master.Labcode;
                TempData["DegreeId"] = laboratories.degreeId;
                return PartialView("~/Views/Reports/_AutonomousCollegesIntakeWithLabsDetails.cshtml", laboratories);
            }

        }


        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College,DataEntry")]
        public ActionResult AutonomousAddEditRecord(Lab laboratories, string cmd)
        {
            //int userID =1;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (laboratories.EquipmentUniqueID == null)
            {
                laboratories.EquipmentUniqueID = string.Empty;
            }
            if (ModelState.IsValid)
            {
                jntuh_college_laboratories jntuh_college_laboratories = new jntuh_college_laboratories();
                jntuh_college_laboratories.CollegeID = laboratories.collegeId;
                jntuh_college_laboratories.EquipmentID = laboratories.EquipmentID;
                jntuh_college_laboratories.Make = laboratories.Make;
                jntuh_college_laboratories.Model = laboratories.Model;
                jntuh_college_laboratories.EquipmentUniqueID = laboratories.EquipmentUniqueID;
                jntuh_college_laboratories.EquipmentName = laboratories.EquipmentName;
                jntuh_college_laboratories.AvailableUnits = laboratories.AvailableUnits;
                jntuh_college_laboratories.AvailableArea = laboratories.AvailableArea;
                jntuh_college_laboratories.RoomNumber = laboratories.RoomNumber;
                jntuh_college_laboratories.EquipmentNo = laboratories.EquipmentNo;
                jntuh_college_laboratories.isActive = true;
                jntuh_college_laboratories.LabName = laboratories.LabName;
                jntuh_college_laboratories.NOPIDOC = laboratories.NoPIDoc;
                jntuh_college_laboratories.NODCdoc = laboratories.NoDCDoc;
                if (laboratories.EquipmentDateOfPurchasing1 != null)
                {
                    laboratories.EquipmentDateOfPurchasing1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing1);
                    jntuh_college_laboratories.EquipmentDateOfPurchasing = Convert.ToDateTime(laboratories.EquipmentDateOfPurchasing1);

                }

                if (laboratories.DelivaryChalanaDate1 != null)
                {
                    laboratories.DelivaryChalanaDate1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate1);
                    jntuh_college_laboratories.DelivaryChalanaDate = Convert.ToDateTime(laboratories.DelivaryChalanaDate1);
                }






                if (cmd == "Save")
                {
                    var existingID = db.jntuh_college_laboratories.Where(c => c.CollegeID == laboratories.collegeId && c.EquipmentID == laboratories.EquipmentID && c.EquipmentNo == laboratories.EquipmentNo).Select(c => c).FirstOrDefault();

                    if (existingID == null)
                    {
                        jntuh_college_laboratories.createdBy = userID;
                        jntuh_college_laboratories.createdOn = DateTime.Now;
                        db.jntuh_college_laboratories.Add(jntuh_college_laboratories);
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
                    jntuh_college_laboratories.id = (int)laboratories.id;
                    jntuh_college_laboratories.createdBy = laboratories.createdBy;
                    jntuh_college_laboratories.createdOn = laboratories.createdOn;
                    jntuh_college_laboratories.updatedBy = userID;
                    jntuh_college_laboratories.updatedOn = DateTime.Now;
                    jntuh_college_laboratories.isActive = true;
                    db.Entry(jntuh_college_laboratories).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Lab Updated Successfully.";
                }
            }
            return RedirectToAction("AutonomusCollegesIntakeWithLabsDetails", new { specializationId = laboratories.specializationId, year = laboratories.year, semester = laboratories.Semester, collegeId = laboratories.collegeId, DegreeIds = TempData["DegreeId"] });
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {

            var lab = db.jntuh_college_laboratories.Where(l => l.id == id).Select(l => l).FirstOrDefault();
            int collegeId = lab.CollegeID;
            var labmaster = db.jntuh_lab_master.Where(l => l.id == lab.EquipmentID).Select(l => l).FirstOrDefault();
            if (lab != null)
            {
                try
                {
                    db.jntuh_college_laboratories.Remove(lab);
                    db.SaveChanges();
                    TempData["Success"] = "Lab Related details Deleted Successfully.";
                }
                catch { }
            }
            if (labmaster.SpecializationID == 39)
            {
                //return RedirectToAction("AllCollegesIntakeWithLabsDetails", new { specializationId = labmaster.SpecializationID, year = labmaster.Year, semester = 1, collegeId, DegreeIds = labmaster.DegreeID });
                return RedirectToAction("AllCollegesIntakeWithLabsDetailsNew", new { collegeId = lab.CollegeID});
            }
            else
            {
                //return RedirectToAction("AllCollegesIntakeWithLabsDetails", new { specializationId = labmaster.SpecializationID, year = labmaster.Year, semester = labmaster.Semester, collegeId, DegreeIds = labmaster.DegreeID });
                return RedirectToAction("AllCollegesIntakeWithLabsDetailsNew", new { collegeId = lab.CollegeID });
            }

            

        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College,DataEntry")]
        public ActionResult AutonomousDeleteRecord(int id)
        {

            var lab = db.jntuh_college_laboratories.Where(l => l.id == id).Select(l => l).FirstOrDefault();
            int collegeId = lab.CollegeID;
            var labmaster = db.jntuh_lab_master.Where(l => l.id == lab.EquipmentID).Select(l => l).FirstOrDefault();
            if (lab != null)
            {
                try
                {
                    db.jntuh_college_laboratories.Remove(lab);
                    db.SaveChanges();
                    TempData["Success"] = "Lab Related details Deleted Successfully.";
                }
                catch { }
            }
            return RedirectToAction("AutonomusCollegesIntakeWithLabsDetails", new { specializationId = labmaster.SpecializationID, year = labmaster.Year, semester = labmaster.Semester, collegeId, DegreeIds = labmaster.DegreeID });

        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            if (academicYearId == AY)
            {
                var val = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.proposedIntake).FirstOrDefault();
                if (val == null)
                {
                    intake = 0;
                }
                else
                {
                    intake = (int)val;
                }

            }
            else
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
            }
            return intake;
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpGet]
        public ActionResult AllCollegesLabsDeficiency(int? rid)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            var colleges = db.jntuh_college_randamcodes.Where(c => c.IsActive == true).Select(c => new
            {
                rid = c.Id,
                RandamCode = c.RandamCode
            }).OrderBy(c => c.RandamCode).ToList();
            ViewBag.Colleges = colleges;
            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });
            if (rid != null)
            {
                int cid = db.jntuh_college_randamcodes.Find(rid).CollegeId;
                ViewBag.display = true;
                int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);

                int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == cid).Select(c => c.id).Take(1).ToArray();
                var jntuh_college_laboratories_deficiency = db.jntuh_college_laboratories_deficiency.Where(c => c.CollegeId == cid).ToList();
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == cid && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                List<Lab> collegeLabMaster = null;
                foreach (var collegeId in collegeIDs)
                {
                    string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault();
                    int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e.specializationId).Distinct().ToArray();
                    int[] DegreeIDs = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == 4 && specializationIds.Contains(l.SpecializationID)).Select(l => l.DegreeID).ToArray();
                    if(CollegeAffiliationStatus=="Yes")
                   {

                       if (DegreeIDs.Contains(4))
                       {
                           collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                            .Where(l => specializationIds.Contains(l.SpecializationID) || l.SpecializationID == 39)
                                                            .Select(l => new Lab
                                                            {
                                                                ////// EquipmentID=l.id,                                                               
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
                                                                //////LabName = l.LabName
                                                            })
                                                            .OrderBy(l => l.degreeDisplayOrder)
                                                            .ThenBy(l => l.department)
                                                            .ThenBy(l => l.specializationName)
                                                            .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                            .ToList();
                       }
                        else
                       {
                           collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                            .Where(l => specializationIds.Contains(l.SpecializationID))
                                                            .Select(l => new Lab
                                                            {
                                                                ////// EquipmentID=l.id,                                                               
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
                                                                //////LabName = l.LabName
                                                            })
                                                            .OrderBy(l => l.degreeDisplayOrder)
                                                            .ThenBy(l => l.department)
                                                            .ThenBy(l => l.specializationName)
                                                            .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                            .ToList();
                       }
                     

                       var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();
                   }
                    else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
                   {

                       if (DegreeIDs.Contains(4))
                       {
                           collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                             .Where(l => ( l.SpecializationID == 39 || specializationIds.Contains(l.SpecializationID)) && l.Labcode != "TMP-CL")
                                                             .Select(l => new Lab
                                                             {
                                                                 ////// EquipmentID=l.id,                                                               
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
                                                                 LabName = l.LabName
                                                             })
                                                             .OrderBy(l => l.degreeDisplayOrder)
                                                             .ThenBy(l => l.department)
                                                             .ThenBy(l => l.specializationName)
                                                             .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                             .ToList();
                       }
                        else
                       {
                          
                           collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                             .Where(l => specializationIds.Contains(l.SpecializationID) && l.Labcode != "TMP-CL")
                                                             .Select(l => new Lab
                                                             {
                                                                 ////// EquipmentID=l.id,                                                               
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
                                                                 LabName = l.LabName
                                                             })
                                                             .OrderBy(l => l.degreeDisplayOrder)
                                                             .ThenBy(l => l.department)
                                                             .ThenBy(l => l.specializationName)
                                                             .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                             .ToList();
                       }
                       

                       var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();
                   }
                  

                    foreach (var item in collegeLabMaster)
                    {
                        Lab lstlabs = new Lab();
                        lstlabs.collegeId = collegeId;
                        lstlabs.EquipmentID = item.EquipmentID;
                        lstlabs.degree = item.degree;
                        lstlabs.department = item.department;
                        lstlabs.specializationName = item.specializationName;
                        lstlabs.specializationId = item.specializationId;
                        lstlabs.Semester = item.Semester;
                        lstlabs.year = item.year;
                        lstlabs.Labcode = item.Labcode;
                        lstlabs.RandomId = (int)rid;
                        //////lstlabs.LabName = item.LabName;
                        lstlabs.EquipmentNo = 1;
                        lstlabs.RandomCode = strcollegecode;
                        lstlabs.degreeDisplayOrder = item.degreeDisplayOrder;
                        //lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId).Select(ld => ld.Id).FirstOrDefault();
                        //if (lstlabs.id != 0)
                        //{
                        //    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId).Select(ld => ld.Deficiency).FirstOrDefault();
                        //    //lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId).Select(ld => ld.Id).FirstOrDefault();
                        //}
                        lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Id).FirstOrDefault();
                        lstlabs.deficiencyStatus = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.DeficiencyStatus).FirstOrDefault();                            
                        if (lstlabs.id != 0)
                        {
                            lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeId && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Deficiency).FirstOrDefault();                            
                        }
                        else
                        {
                            lstlabs.deficiency = null;
                            lstlabs.id = 0;
                        }
                        lstlaboratories.Add(lstlabs);
                    }
                }
                lstlaboratories = lstlaboratories.OrderBy(l => l.degreeDisplayOrder)
                                                                .ThenBy(l => l.department)
                                                                .ThenBy(l => l.specializationName)
                                                                .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                                .ToList();
            }
            else
            {
                ViewBag.display = false;
            }

            return View("~/Views/Reports/AllCollegesLabsDeficiency.cshtml", lstlaboratories);
        }
        //Commented on 14-06-2018 by Narayana Reddy
        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        //[HttpPost]
        //public ActionResult AllCollegesLabsDeficiency(List<Lab> labs)
        //{
        //    int RandomId = labs.Select(c => c.RandomId).FirstOrDefault();
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    // var labsItems = labs.Where(ld => ld.deficiency != null).ToList();
        //    var labsItems = labs.ToList();
        //    if (labsItems.Count() > 0)
        //    {

        //        foreach (var item in labsItems)
        //        {
        //            jntuh_college_laboratories_deficiency labsDeficiency = new jntuh_college_laboratories_deficiency();
        //            labsDeficiency.CollegeId = item.collegeId;
        //            labsDeficiency.LabCode = item.Labcode ?? string.Empty;
        //            labsDeficiency.IsActive = true;
        //            labsDeficiency.SpecializationId = item.specializationId;
        //            labsDeficiency.Year = item.year;
        //            labsDeficiency.Semister = (int)item.Semester;

        //            if (item.deficiency == null)
        //            {
        //                labsDeficiency.Deficiency = true;
        //                labsDeficiency.DeficiencyStatus = true;

        //            }
        //            else
        //            {
        //                labsDeficiency.Deficiency = (bool)item.deficiency;
        //                labsDeficiency.DeficiencyStatus = false;
        //            }
        //            if (item.id == 0)
        //            {
        //                labsDeficiency.CreatedBy = userID;
        //                labsDeficiency.CreatedOn = DateTime.Now;
        //                db.jntuh_college_laboratories_deficiency.Add(labsDeficiency);
        //                try
        //                {
        //                    db.SaveChanges();
        //                }
        //                catch (DbEntityValidationException dbEx)
        //                {
        //                    foreach (var validationErrors in dbEx.EntityValidationErrors)
        //                    {
        //                        foreach (var validationError in validationErrors.ValidationErrors)
        //                        {
        //                            Trace.TraceInformation("Property: {0} Error: {1}",
        //                                                    validationError.PropertyName,
        //                                                    validationError.ErrorMessage);
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                jntuh_college_laboratories_deficiency labsDeficiencyupdate = db.jntuh_college_laboratories_deficiency.Find(item.id);
        //                labsDeficiencyupdate.LabCode = item.Labcode ?? string.Empty;
        //                labsDeficiencyupdate.Deficiency = (bool)item.deficiency;
        //                if (item.deficiency==false)
        //                    labsDeficiencyupdate.DeficiencyStatus = false;

        //                labsDeficiencyupdate.UpdatedBy = userID;
        //                labsDeficiencyupdate.UpdatedOn = DateTime.Now;
        //                db.Entry(labsDeficiencyupdate).State = EntityState.Modified;
        //                try
        //                {
        //                    db.SaveChanges();
        //                }
        //                catch (DbEntityValidationException dbEx)
        //                {
        //                    foreach (var validationErrors in dbEx.EntityValidationErrors)
        //                    {
        //                        foreach (var validationError in validationErrors.ValidationErrors)
        //                        {
        //                            Trace.TraceInformation("Property: {0} Error: {1}",
        //                                                    validationError.PropertyName,
        //                                                    validationError.ErrorMessage);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        TempData["Success"] = "Data Saved";


        //        //TempData["Error"] = "Invalid Data";

        //    }
        //    return RedirectToAction("AllCollegesLabsDeficiency", new { rid = RandomId });
        //}



        //public ActionResult AllCollegePhysicalLabs(int? CollegeId)
        //{
        //    var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.IsCollegeEditable == false).Select(c => new
        //    {
        //        collegeId = c.co.id,
        //        collegeName = c.co.collegeCode + "-" + c.co.collegeName
        //    }).OrderBy(c => c.collegeName).ToList();

        //    colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });
        //    ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
        //    List<LabsController.physicalLab> ObjPhysicalLab = new List<LabsController.physicalLab>();
        //    if (CollegeId != null)
        //    {
               
        //        string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == CollegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
        //        var jntuh_physical_lab = db.jntuh_physical_labmaster.Where(k => k.Collegeid == CollegeId).Select(k => new { Id = k.Id, LabCode = k.Labcode.Trim().ToUpper(), deptId = k.DepartmentId }).ToList();
        //        if (CollegeAffiliationStatus == "Yes")
        //        {
        //            ObjPhysicalLab = (from lab in db.jntuh_lab_master
        //                              join deg in db.jntuh_degree on lab.DegreeID equals deg.id
        //                              join dep in db.jntuh_department on lab.DepartmentID equals dep.id
        //                              join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
        //                              where lab.CollegeId == CollegeId && deg.id == 4
        //                              select new LabsController.physicalLab
        //                              {
        //                                  Labid = lab.id,
        //                                  collegeId = (int)lab.CollegeId,
        //                                  degreeid = lab.DegreeID,
        //                                  departmentid = lab.DepartmentID,
        //                                  specializationid = lab.SpecializationID,
        //                                  degree = deg.degree,
        //                                  specialization = spec.specializationName,
        //                                  department = dep.departmentName,
        //                                  year = lab.Year,
        //                                  semister = lab.Semester,
        //                                  Labname = lab.LabName,
        //                                  LabCode = lab.Labcode.Trim().ToUpper()
        //                              }).ToList();
        //        }
        //        else
        //        {

        //            var jntuh_specialization = db.jntuh_specialization.ToList();



        //            int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == CollegeId && e.academicYearId == 8).Select(e => e.specializationId).ToArray();

        //            var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

        //            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();


        //            if (DegreeIds.Contains(4))
        //            {


        //                if (specializationIDs.Contains(134))
        //                {
        //                    ObjPhysicalLab = (from lab in db.jntuh_lab_master
        //                                      join deg in db.jntuh_degree on lab.DegreeID equals deg.id
        //                                      join dep in db.jntuh_department on lab.DepartmentID equals dep.id
        //                                      join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
        //                                      where lab.CollegeId == null && deg.id == 4 && (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34)
        //                                      select new LabsController.physicalLab
        //                                      {
        //                                          Labid = lab.id,
        //                                          collegeId = (int)CollegeId,
        //                                          degreeid = lab.DegreeID,
        //                                          departmentid = lab.DepartmentID,
        //                                          specializationid = lab.SpecializationID,
        //                                          degree = deg.degree,
        //                                          specialization = spec.specializationName,
        //                                          department = dep.departmentName,
        //                                          year = lab.Year,
        //                                          semister = lab.Semester,
        //                                          Labname = lab.LabName,
        //                                          LabCode = lab.Labcode.Trim().ToUpper()
        //                                      }).ToList();
        //                }
        //                else
        //                {
        //                    ObjPhysicalLab = (from lab in db.jntuh_lab_master
        //                                      join deg in db.jntuh_degree on lab.DegreeID equals deg.id
        //                                      join dep in db.jntuh_department on lab.DepartmentID equals dep.id
        //                                      join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
        //                                      where lab.CollegeId == null && deg.id == 4 && (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34) && lab.SpecializationID != 134
        //                                      select new LabsController.physicalLab
        //                                      {
        //                                          Labid = lab.id,
        //                                          collegeId = (int)CollegeId,
        //                                          degreeid = lab.DegreeID,
        //                                          departmentid = lab.DepartmentID,
        //                                          specializationid = lab.SpecializationID,
        //                                          degree = deg.degree,
        //                                          specialization = spec.specializationName,
        //                                          department = dep.departmentName,
        //                                          year = lab.Year,
        //                                          semister = lab.Semester,
        //                                          Labname = lab.LabName,
        //                                          LabCode = lab.Labcode.Trim().ToUpper()
        //                                      }).ToList();
        //                }

        //            }
        //            else
        //            {
        //                ObjPhysicalLab = (from lab in db.jntuh_lab_master
        //                                  join deg in db.jntuh_degree on lab.DegreeID equals deg.id
        //                                  join dep in db.jntuh_department on lab.DepartmentID equals dep.id
        //                                  join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
        //                                  where lab.CollegeId == null && deg.id == 4 && DepartmentsData.Contains(lab.DepartmentID)
        //                                  select new LabsController.physicalLab
        //                                  {
        //                                      Labid = lab.id,
        //                                      collegeId = (int)CollegeId,
        //                                      degreeid = lab.DegreeID,
        //                                      departmentid = lab.DepartmentID,
        //                                      specializationid = lab.SpecializationID,
        //                                      degree = deg.degree,
        //                                      specialization = spec.specializationName,
        //                                      department = dep.departmentName,
        //                                      year = lab.Year,
        //                                      semister = lab.Semester,
        //                                      Labname = lab.LabName,
        //                                      LabCode = lab.Labcode.Trim().ToUpper()
        //                                  }).ToList();
        //            }



        //        }







        //        ObjPhysicalLab = ObjPhysicalLab.GroupBy(e => new { e.LabCode, e.departmentid }).Select(e => new LabsController.physicalLab
        //        {
        //            Labid = e.FirstOrDefault().Labid,
        //            collegeId = e.FirstOrDefault().collegeId,
        //            degreeid = e.FirstOrDefault().degreeid,
        //            departmentid = e.FirstOrDefault().departmentid,
        //            specializationid = e.FirstOrDefault().specializationid,
        //            degree = e.FirstOrDefault().degree,
        //            specialization = e.FirstOrDefault().specialization,
        //            department = e.FirstOrDefault().department,
        //            year = e.FirstOrDefault().year,
        //            semister = e.FirstOrDefault().semister,
        //            Labname = e.FirstOrDefault().Labname,
        //            LabCode = e.FirstOrDefault().LabCode,

        //            physicalId = jntuh_physical_lab.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid).Select(d => d.Id).FirstOrDefault()

        //        }).ToList();
        //    }
        //    return View("~/Views/Reports/AllCollegePhysicalLabs.cshtml", ObjPhysicalLab);
        //}


        #region Physical Labs Details
        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AllCollegePhysicalLabs(int? CollegeId)
        {
            int[] PharmacyIds = { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445, 448, 454 };
            int[] MBAIds = { 5, 67, 119, 174, 246, 296, 343, 355, 386, 394, 411, 413, 421, 424, 430, 449, 452 };
           
            var actualYear1 =
                 db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
             .Select(a => a.actualYear)
             .FirstOrDefault();
            var AcademicYearId1 =
                db.jntuh_academic_year.Where(d => d.actualYear == (actualYear1 + 1)).Select(z => z.id).FirstOrDefault();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
            if (userRoles.Contains(
                db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                    .Select(r => r.id)
                    .FirstOrDefault()))
            {
                var colleges =
                    db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                        (co, e) => new {co = co, e = e})
                        .Where(c => c.co.isActive == true && c.e.academicyearId == AcademicYearId1 && c.e.IsCollegeEditable == false)
                        .Select(c => new
                        {
                            collegeId = c.co.id,
                            collegeName = c.co.collegeCode + "-" + c.co.collegeName
                        }).OrderBy(c => c.collegeName).ToList();

                colleges.Add(new {collegeId = 0, collegeName = "00-ALL Colleges"});
                ViewBag.Colleges =
                    colleges.Where(q => !PharmacyIds.Contains(q.collegeId) && !MBAIds.Contains(q.collegeId))
                        .OrderBy(c => c.collegeId)
                        .ThenBy(c => c.collegeName)
                        .ToList();
            }
            else
            {
                int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Data Entry").Select(p => p.id).SingleOrDefault();
                int[] assignedcollegeslist =
                    db.jntuh_dataentry_allotment.Where(
                        d =>
                            d.InspectionPhaseId == InspectionPhaseId && d.userID == userID && d.isActive == true &&
                            d.isCompleted == false).Select(s => s.collegeID).ToArray();
                ViewBag.Colleges =
                    db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
                        (co, e) => new { co = co, e = e })
                        .Where(c => c.e.academicyearId == AcademicYearId1 && c.e.IsCollegeEditable == false && assignedcollegeslist.Contains(c.e.collegeId))
                        .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
                        .OrderBy(c => c.collegeName)
                        .ToList();
            }
            
            List<LabsController.physicalLab> ObjPhysicalLab = new List<LabsController.physicalLab>();

            ViewBag.TotalLabs = null;
            ViewBag.UpdatedLabs = null;
            ViewBag.NotUpdatedLabs = null;


            if (CollegeId != null)
            {

                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == CollegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                var jntuh_physical_lab = db.jntuh_physical_labmaster.Where(k => k.Collegeid == CollegeId).Select(k => new { Id = k.Id, LabCode = k.Labcode.Trim().ToUpper(), deptId = k.DepartmentId, NoofLabs = k.Numberofavilablelabs }).ToList();
                var jntuh_physical_lab_copy = db.jntuh_physical_labmaster_copy.Where(k => k.Collegeid == CollegeId).Select(k => new { Id = k.Id, LabCode = k.Labcode.Trim().ToUpper(), deptId = k.DepartmentId, NoofLabs = k.Numberofavilablelabs, NoofRequiredLabs = k.Numberofrequiredlabs }).ToList();
                if (CollegeAffiliationStatus == "Yes")
                {
                    ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                      join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                      join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                      join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                      where lab.CollegeId == CollegeId && deg.id == 4
                                      select new LabsController.physicalLab
                                      {
                                          Labid = lab.id,
                                          collegeId = (int)lab.CollegeId,
                                          degreeid = lab.DegreeID,
                                          departmentid = lab.DepartmentID,
                                          specializationid = lab.SpecializationID,
                                          degree = deg.degree,
                                          specialization = spec.specializationName,
                                          department = dep.departmentName,
                                          year = lab.Year,
                                          semister = lab.Semester,
                                          Labname = lab.LabName,
                                          LabCode = lab.Labcode.Trim().ToUpper()
                                      }).ToList();
                }
                else
                {

                    var jntuh_specialization = db.jntuh_specialization.ToList();



                    int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == CollegeId && e.academicYearId == 9).Select(e => e.specializationId).ToArray();

                    var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                    var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();


                    if (DegreeIds.Contains(4))
                    {


                        if (specializationIDs.Contains(134))
                        {
                            ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                              join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                              join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                              join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                              where lab.CollegeId == null && deg.id == 4 && (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34)
                                              select new LabsController.physicalLab
                                              {
                                                  Labid = lab.id,
                                                  collegeId = (int)CollegeId,
                                                  degreeid = lab.DegreeID,
                                                  departmentid = lab.DepartmentID,
                                                  specializationid = lab.SpecializationID,
                                                  degree = deg.degree,
                                                  specialization = spec.specializationName,
                                                  department = dep.departmentName,
                                                  year = lab.Year,
                                                  semister = lab.Semester,
                                                  Labname = lab.LabName,
                                                  LabCode = lab.Labcode.Trim().ToUpper()
                                              }).ToList();
                        }
                        else
                        {
                            ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                              join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                              join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                              join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                              where lab.CollegeId == null && deg.id == 4 && (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34) && lab.SpecializationID != 134
                                              select new LabsController.physicalLab
                                              {
                                                  Labid = lab.id,
                                                  collegeId = (int)CollegeId,
                                                  degreeid = lab.DegreeID,
                                                  departmentid = lab.DepartmentID,
                                                  specializationid = lab.SpecializationID,
                                                  degree = deg.degree,
                                                  specialization = spec.specializationName,
                                                  department = dep.departmentName,
                                                  year = lab.Year,
                                                  semister = lab.Semester,
                                                  Labname = lab.LabName,
                                                  LabCode = lab.Labcode.Trim().ToUpper()
                                              }).ToList();
                        }

                    }
                    else
                    {
                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                          join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                          join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                          join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                          where lab.CollegeId == null && deg.id == 4 && DepartmentsData.Contains(lab.DepartmentID)
                                          select new LabsController.physicalLab
                                          {
                                              Labid = lab.id,
                                              collegeId = (int)CollegeId,
                                              degreeid = lab.DegreeID,
                                              departmentid = lab.DepartmentID,
                                              specializationid = lab.SpecializationID,
                                              degree = deg.degree,
                                              specialization = spec.specializationName,
                                              department = dep.departmentName,
                                              year = lab.Year,
                                              semister = lab.Semester,
                                              Labname = lab.LabName,
                                              LabCode = lab.Labcode.Trim().ToUpper()
                                          }).ToList();
                    }



                }

                ObjPhysicalLab = ObjPhysicalLab.GroupBy(e => new { e.LabCode, e.departmentid }).Select(e => new LabsController.physicalLab
                {
                    Labid = e.FirstOrDefault().Labid,
                    collegeId = e.FirstOrDefault().collegeId,
                    degreeid = e.FirstOrDefault().degreeid,
                    departmentid = e.FirstOrDefault().departmentid,
                    specializationid = e.FirstOrDefault().specializationid,
                    degree = e.FirstOrDefault().degree,
                    specialization = e.FirstOrDefault().specialization,
                    department = e.FirstOrDefault().department,
                    year = e.FirstOrDefault().year,
                    semister = e.FirstOrDefault().semister,
                    Labname = e.FirstOrDefault().Labname,
                    LabCode = e.FirstOrDefault().LabCode,

                    physicalId = jntuh_physical_lab_copy.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid ).Select(a => a.Id).FirstOrDefault() == null ? 0 : jntuh_physical_lab_copy.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid).Select(a => a.Id).FirstOrDefault(),
                    NoOfAvailabeLabs = jntuh_physical_lab_copy.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid && d.NoofLabs != null).Select(a => a.NoofLabs).FirstOrDefault() == null ? 0 : jntuh_physical_lab_copy.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid && d.NoofLabs != null).Select(a => a.NoofLabs).FirstOrDefault()
                    //NoOfRequiredLabs = jntuh_physical_lab_copy.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid && d.NoofRequiredLabs != null).Select(a => a.NoofRequiredLabs).FirstOrDefault() == null ? 0 : jntuh_physical_lab_copy.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid && d.NoofRequiredLabs != null).Select(a => a.NoofRequiredLabs).FirstOrDefault()
                    // physicalId = jntuh_physical_lab.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid).Select(d => d.Id).FirstOrDefault(),
                    //NoOfAvailabeLabs = jntuh_physical_lab.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid).Select(d => d.NoofLabs).FirstOrDefault()
                }).OrderBy(d => d.department).ThenBy(e => e.degree).ToList();

                ViewBag.TotalLabs = ObjPhysicalLab.Count();
                ViewBag.UpdatedLabs = ObjPhysicalLab.Where(e => e.physicalId != 0).Count();
                ViewBag.NotUpdatedLabs = ObjPhysicalLab.Where(e => e.physicalId == 0 || e.physicalId == null).Count();
            }

            return View("~/Views/Reports/AllCollegePhysicalLabs.cshtml", ObjPhysicalLab);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpGet]
        public ActionResult AddEditPhysicalLabRecord(int? PhysicalLabId, int LabId, int CollegeId)
        {
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == CollegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            if (PhysicalLabId != null && PhysicalLabId != 0)
            {
                LabsController.physicalLab ObjPhysicalLab = (from phys in db.jntuh_physical_labmaster_copy
                                                             join deg in db.jntuh_degree on phys.DegreeId equals deg.id
                                                             join dep in db.jntuh_department on phys.DepartmentId equals dep.id
                                                             join spec in db.jntuh_specialization on phys.SpecializationId equals spec.id
                                                             where phys.Id == PhysicalLabId && phys.Collegeid == CollegeId
                                                             select new LabsController.physicalLab
                                                             {
                                                                 id = phys.Id,
                                                                 collegeId = phys.Collegeid,
                                                                 degreeid = phys.DegreeId,
                                                                 departmentid = phys.DepartmentId,
                                                                 specializationid = phys.SpecializationId,
                                                                 degree = deg.degree,
                                                                 specialization = spec.specializationName,
                                                                 department = dep.departmentName,
                                                                 year = phys.Year,
                                                                 semister = phys.Semister,
                                                                 Labname = phys.LabName,
                                                                 LabCode = phys.Labcode,
                                                                 NoOfRequiredLabs = phys.Numberofrequiredlabs,
                                                                 NoOfAvailabeLabs = phys.Numberofavilablelabs,
                                                                 Remarks = phys.Remarks
                                                             }).FirstOrDefault();

                return PartialView("~/Views/Reports/_AddEditPhysicalLabRecord.cshtml", ObjPhysicalLab);

            }
            else
            {
                LabsController.physicalLab ObjPhysicalLab = new LabsController.physicalLab();
                if (CollegeAffiliationStatus == "Yes")
                {
                    ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                      join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                      join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                      join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                      where lab.id == LabId && lab.CollegeId == CollegeId
                                      select new LabsController.physicalLab
                                      {
                                          Labid = lab.id,
                                          collegeId = (int)lab.CollegeId,
                                          degreeid = lab.DegreeID,
                                          departmentid = lab.DepartmentID,
                                          specializationid = lab.SpecializationID,
                                          degree = deg.degree,
                                          specialization = spec.specializationName,
                                          department = dep.departmentName,
                                          year = lab.Year,
                                          LabCode = lab.Labcode,
                                          semister = lab.Semester,
                                          Labname = lab.LabName
                                      }).FirstOrDefault();
                }
                else
                {

                    ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                      join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                      join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                      join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                      where lab.id == LabId && lab.CollegeId == null
                                      select new LabsController.physicalLab
                                      {
                                          Labid = lab.id,
                                          collegeId = CollegeId,
                                          degreeid = lab.DegreeID,
                                          departmentid = lab.DepartmentID,
                                          specializationid = lab.SpecializationID,
                                          degree = deg.degree,
                                          specialization = spec.specializationName,
                                          department = dep.departmentName,
                                          year = lab.Year,
                                          LabCode = lab.Labcode,
                                          semister = lab.Semester,
                                          Labname = lab.LabName
                                      }).FirstOrDefault();
                }


                return PartialView("~/Views/Reports/_AddEditPhysicalLabRecord.cshtml", ObjPhysicalLab);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry,FacultyVerification")]
        public ActionResult AddEditPhysicalLabRecord(LabsController.physicalLab physicallab)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (ModelState.IsValid)
            {
                
                if (physicallab.id == 0 || physicallab.id == null)
                {
                    jntuh_physical_labmaster_copy toAdd = new jntuh_physical_labmaster_copy();
                    toAdd.Collegeid = physicallab.collegeId;
                    toAdd.Year = physicallab.year;
                    toAdd.Semister = physicallab.semister;
                    toAdd.DegreeId = physicallab.degreeid;
                    toAdd.DepartmentId = physicallab.departmentid;
                    toAdd.SpecializationId = physicallab.specializationid;
                    toAdd.Labcode = physicallab.LabCode;
                    toAdd.LabName = physicallab.Labname;
                    toAdd.Numberofrequiredlabs = physicallab.NoOfRequiredLabs;
                    toAdd.Numberofavilablelabs = physicallab.NoOfAvailabeLabs;
                    toAdd.Remarks = physicallab.Remarks;
                    toAdd.Createdby = userID;
                    toAdd.Createdon = DateTime.Now;
                    db.jntuh_physical_labmaster_copy.Add(toAdd);
                    db.SaveChanges();
                    TempData["Success"] = "Added Successfully";

                }
                else
                {
                    jntuh_physical_labmaster_copy toUpdate = db.jntuh_physical_labmaster_copy.Find(physicallab.id);

                    toUpdate.DegreeId = physicallab.degreeid;
                    toUpdate.DepartmentId = physicallab.departmentid;
                    toUpdate.LabName = physicallab.Labname;
                    //toUpdate.Numberofrequiredlabs = physicallab.NoOfRequiredLabs;
                    toUpdate.Numberofavilablelabs = physicallab.NoOfAvailabeLabs;
                    toUpdate.Remarks = physicallab.Remarks;
                    toUpdate.Updatedby = userID;
                    toUpdate.UpdatedOn = DateTime.Now;
                    db.Entry(toUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated Successfully";

                }
            }
            return RedirectToAction("AllCollegePhysicalLabs", new { CollegeId = physicallab.collegeId });
        }


        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry,FacultyVerification")]
        public ActionResult DeletePhysicalLabRecord(int? PhysicalLabId, int LabId, int CollegeId)
        {
            var lab = db.jntuh_physical_labmaster_copy.Where(l => l.Id == PhysicalLabId && l.Collegeid == CollegeId).Select(l => l).FirstOrDefault();
            //int collegeId = lab.CollegeID;
            //var labmaster = db.jntuh_lab_master.Where(l => l.id == lab.EquipmentID).Select(l => l).FirstOrDefault();
            if (lab != null)
            {
                try
                {
                    lab.Numberofrequiredlabs = null;
                    db.Entry(lab).State = EntityState.Modified;
                    db.SaveChanges();
                    //db.jntuh_physical_labmaster_copy.Remove(lab);
                    db.SaveChanges();
                    TempData["Success"] = "Lab Deleted Successfully.";
                }
                catch
                {

                }
            }
            return RedirectToAction("AllCollegePhysicalLabs", new { CollegeId = CollegeId });
        }
        #endregion

       



        public class CollegesIntakeWithLabs
        {
            public int collegeId { get; set; }
            public string collegeCode { get; set; }
            public string collegeName { get; set; }

            public int approvedIntake1 { get; set; }
            public int approvedIntake2 { get; set; }
            public int approvedIntake3 { get; set; }
            public int approvedIntake4 { get; set; }
            public int DegreeID { get; set; }

            public string Degree { get; set; }
            public string Department { get; set; }
            public string Specialization { get; set; }

            public int specializationId { get; set; }
            public int year { get; set; }
            public int semester { get; set; }

            public int shiftId { get; set; }
            public int? degreeDisplayOrder { get; set; }

            public int requiredLabs41 { get; set; }
            public int uplodedLabs41 { get; set; }
            public int notuplodedLabs41 { get; set; }
            public int Autonomus41 { get; set; }

            public int requiredLabs42 { get; set; }
            public int uplodedLabs42 { get; set; }
            public int notuplodedLabs42 { get; set; }
            public int Autonomus42 { get; set; }

            public int requiredLabs31 { get; set; }
            public int uplodedLabs31 { get; set; }
            public int notuplodedLabs31 { get; set; }
            public int Autonomus31 { get; set; }

            public int requiredLabs32 { get; set; }
            public int uplodedLabs32 { get; set; }
            public int notuplodedLabs32 { get; set; }
            public int Autonomus32 { get; set; }


            public int requiredLabs21 { get; set; }
            public int uplodedLabs21 { get; set; }
            public int notuplodedLabs21 { get; set; }
            public int Autonomus21 { get; set; }

            public int requiredLabs22 { get; set; }
            public int uplodedLabs22 { get; set; }
            public int notuplodedLabs22 { get; set; }
            public int Autonomus22 { get; set; }

            public int requiredLabs11 { get; set; }
            public int uplodedLabs11 { get; set; }
            public int notuplodedLabs11 { get; set; }
            public int Autonomus11 { get; set; }

            public int requiredLabs12 { get; set; }
            public int uplodedLabs12 { get; set; }
            public int notuplodedLabs12 { get; set; }
            public int Autonomus12 { get; set; }

        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        [HttpGet]
        public ActionResult LabsVideoVerification(int? collegeId, string type)
        {
            var colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true && c.e.IsCollegeEditable == false).Select(c => new
            {
                collegeId = c.co.id,
                collegeName = c.co.collegeCode + "-" + c.co.collegeName
            }).OrderBy(c => c.collegeName).ToList();

            colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            int[] AppealCollegeIds = db.jntuh_appeal_college_edit_status.Select(C => C.collegeId).ToArray();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegesIntakeWithLabs> intakedetailsList = new List<CollegesIntakeWithLabs>();
            if (collegeId != null)
            {
                int[] collegeIDs = null;
                if (collegeId != 0)
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                }
                else
                {
                    collegeIDs = db.jntuh_college.Where(c => AppealCollegeIds.Contains(c.id) && c.isActive == true).Select(c => c.id).ToArray();
                }
                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

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
                    newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                    newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                    newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                    newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    collegeIntakeExisting.Add(newIntake);
                }
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();
                var jntuh_lab_master = db.jntuh_lab_master.ToList();

                // Old Code Commented By Srinivas.T
                //var jntuh_college_laboratories = db.jntuh_college_laboratories.ToList();
                //var jntuh_colleg = db.jntuh_college.ToList();

                var jntuh_college_laboratories = db.jntuh_college_laboratories.Where(C => collegeIDs.Contains(C.CollegeID)).ToList();
                var jntuh_colleg = db.jntuh_college.Where(C => collegeIDs.Contains(C.id)).ToList();

                int SpecializationId = 0;
                int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
                int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
                if (DegreeIds.Contains(4))
                    SpecializationId = 39;
                else
                    SpecializationId = 0;
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                int DegreeID = 0, Year = 0;
                foreach (var item in collegeIntakeExisting)
                {
                    CollegesIntakeWithLabs intakedetails = new CollegesIntakeWithLabs();
                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    intakedetails.Degree = item.Degree;
                    intakedetails.DegreeID = item.degreeID;
                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_colleg.Where(c => c.isActive == true && c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_colleg.Where(c => c.isActive == true && c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.shiftId = item.shiftId;
                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;


                    #region Old code Commented By Srinivas
                    //if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
                    //{
                    //    intakedetails.requiredLabs41 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 1).Count();
                    //    intakedetails.uplodedLabs41 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 1).Count();
                    //    intakedetails.notuplodedLabs41 = intakedetails.requiredLabs41 - intakedetails.uplodedLabs41;

                    //    intakedetails.requiredLabs42 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 2).Count();
                    //    intakedetails.uplodedLabs42 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 2).Count();
                    //    intakedetails.notuplodedLabs42 = intakedetails.requiredLabs42 - intakedetails.uplodedLabs42;

                    //    intakedetails.requiredLabs31 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 1).Count();
                    //    intakedetails.uplodedLabs31 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 1).Count();
                    //    intakedetails.notuplodedLabs31 = intakedetails.requiredLabs31 - intakedetails.uplodedLabs31;

                    //    intakedetails.requiredLabs32 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 2).Count();
                    //    intakedetails.uplodedLabs32 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 2).Count();
                    //    intakedetails.notuplodedLabs32 = intakedetails.requiredLabs32 - intakedetails.uplodedLabs32;

                    //    intakedetails.requiredLabs21 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 1).Count();
                    //    intakedetails.uplodedLabs21 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 1).Count();
                    //    intakedetails.notuplodedLabs21 = intakedetails.requiredLabs21 - intakedetails.uplodedLabs21;

                    //    intakedetails.requiredLabs22 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 2).Count();
                    //    intakedetails.uplodedLabs22 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 2).Count();
                    //    intakedetails.notuplodedLabs22 = intakedetails.requiredLabs22 - intakedetails.uplodedLabs22;


                    //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1).Count();
                    //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).Count();
                    //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;

                    //    intakedetails.requiredLabs12 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 2).Count();
                    //    intakedetails.uplodedLabs12 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 2).Count();
                    //    intakedetails.notuplodedLabs12 = intakedetails.requiredLabs12 - intakedetails.uplodedLabs12;

                    //}
                    //else
                    //{
                    //    intakedetails.requiredLabs41 = 0;
                    //    intakedetails.uplodedLabs41 = 0;
                    //    intakedetails.notuplodedLabs41 = 0;

                    //    intakedetails.requiredLabs42 = 0;
                    //    intakedetails.uplodedLabs42 = 0;
                    //    intakedetails.notuplodedLabs42 = 0;

                    //    intakedetails.requiredLabs31 = 0;
                    //    intakedetails.uplodedLabs31 = 0;
                    //    intakedetails.notuplodedLabs31 = 0;

                    //    intakedetails.requiredLabs32 = 0;
                    //    intakedetails.uplodedLabs32 = 0;
                    //    intakedetails.notuplodedLabs32 = 0;

                    //    intakedetails.requiredLabs21 = 0;
                    //    intakedetails.uplodedLabs21 = 0;
                    //    intakedetails.notuplodedLabs21 = 0;

                    //    intakedetails.requiredLabs22 = 0;
                    //    intakedetails.uplodedLabs22 = 0;
                    //    intakedetails.notuplodedLabs22 = 0;

                    //    intakedetails.requiredLabs11 = 4;
                    //    //intakedetails.uplodedLabs11 = 0;
                    //    //&& l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1
                    //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId).Count();
                    //    intakedetails.notuplodedLabs11 = 4 - intakedetails.uplodedLabs11;

                    //    intakedetails.requiredLabs12 = 0;
                    //    intakedetails.uplodedLabs12 = 0;
                    //    intakedetails.notuplodedLabs12 = 0;
                    //}
                    //intakedetailsList.Add(intakedetails);
                    #endregion

                    #region New code Written By Srinivas
                    int[] degids = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId).Select(l => l.jntuh_lab_master.DegreeID).Distinct().ToArray();
                    DegreeID = item.degreeID;

                    Year = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId).Select(l => l.Year).FirstOrDefault();
                    if (CollegeAffiliationStatus == "Yes")
                    {
                        intakedetails.requiredLabs41 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs41 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                        // intakedetails.Autonomus41 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs41 = intakedetails.requiredLabs41 - intakedetails.uplodedLabs41;

                        intakedetails.requiredLabs42 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 2 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs42 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 2 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                        //intakedetails.Autonomus42 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 2 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs42 = intakedetails.requiredLabs42 - intakedetails.uplodedLabs42;

                        intakedetails.requiredLabs31 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs31 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                        // intakedetails.Autonomus31 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs31 = intakedetails.requiredLabs31 - intakedetails.uplodedLabs31;

                        intakedetails.requiredLabs32 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 2 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs32 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 2 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                        // intakedetails.Autonomus32 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 2 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs32 = intakedetails.requiredLabs32 - intakedetails.uplodedLabs32;

                        intakedetails.requiredLabs21 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs21 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        // intakedetails.Autonomus21 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs21 = intakedetails.requiredLabs21 - intakedetails.uplodedLabs21;

                        intakedetails.requiredLabs22 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 2 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs22 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 2).GroupBy(l => l.EquipmentID).Count();
                        //  intakedetails.Autonomus22 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 2 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs22 = intakedetails.requiredLabs22 - intakedetails.uplodedLabs22;


                        //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode != "TMP-CL").Count();
                        //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).Count();
                        //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;

                        if (DegreeID == 1)
                        {
                            //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 1 && l.SpecializationID == item.specializationId && l.CollegeId == item.collegeId)).Count();
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || (l.jntuh_lab_master.Year == 0 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.SpecializationID == item.specializationId)).GroupBy(l => l.EquipmentID).Count(); 
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();

                            // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL" || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 1 && l.SpecializationID == item.specializationId && l.Labcode == "TMP-CL")).Count();  //  && l.Year == 1 && l.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 2)
                        {
                            //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 2 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 2 && l.SpecializationID == item.specializationId && l.CollegeId == item.collegeId)).Count(); // || (l.Year == 1 || l.Semester == 1)   && l.Year == 1 && l.Semester == 1
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1  || (l.jntuh_lab_master.Year == 0 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.SpecializationID == item.specializationId)).GroupBy(l => l.EquipmentID).Count(); //|| (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)  && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 2 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();

                            // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 2 && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL" || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 2 && l.SpecializationID == item.specializationId && l.Labcode == "TMP-CL")).Count(); // || (l.Year == 1 || l.Semester == 1)   && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 3)
                        {

                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.DegreeID == 3 && l.Year == 1 && l.Semester == 1 && l.CollegeId == item.collegeId).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 3).GroupBy(l => l.EquipmentID).Count();
                            // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.DegreeID == 3 && l.Labcode == "TMP-CL").Count();
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 4)
                        {
                            //if (Year == 1)
                            //{
                            //     intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 || l.SpecializationID == 39).Count();
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            //else
                            //{
                            //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1).Count();
                            //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}


                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == 39 && l.CollegeId == item.collegeId).Count();  //l.SpecializationID == item.specializationId &&  l.Semester == 1 && 
                            // intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.Year == 1  && l.SpecializationID == 39).Count(); 
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.Year == 1 || l.jntuh_lab_master.Year == 3&&l.jntuh_lab_master.Semester==0 && l.jntuh_lab_master.SpecializationID == 39 && l.jntuh_lab_master.Labcode != "TMP-CL").GroupBy(l => l.EquipmentID).Count();   // && l.jntuh_lab_master.SpecializationID == item.specializationId  l.jntuh_lab_master.Semester == 1 &&
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == 39 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();   // && l.jntuh_lab_master.Year == 1 || l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 0
                            //intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == 39 && l.Year == 1 && l.Labcode == "TMP-CL").Count();
                            //new  intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID==item.specializationId &&l.Labcode == "TMP-CL" && l.Year == 0 && l.Semester == 0).Count();

                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;


                            //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.Year == 1 && l.Semester == 0 && l.Labcode != "TMP-CL" && l.SpecializationID == 39).Count();
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.Labcode != "TMP-CL" && l.jntuh_lab_master.SpecializationID == 39).Count();
                            //intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                            //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;


                            //if (Year == 1)
                            //{
                            //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l =>  l.Year == 1 && l.Semester == 1 && l.Labcode != "TMP-CL" && l.SpecializationID == 39).Count();
                            //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.Labcode != "TMP-CL" || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //    intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                            //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            //else
                            //{
                            //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode != "TMP-CL" ).Count();
                            //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.Labcode != "TMP-CL" ).Count();
                            //    intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.Labcode == "TMP-CL").Count();
                            //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}


                        }
                        else if (DegreeID == 5)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 5 && l.CollegeId == item.collegeId).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 5 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 5 && l.Labcode == "TMP-CL").Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 6)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 6 && l.CollegeId == item.collegeId && l.Year == 1 && l.Semester == 1).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 6 && l.jntuh_lab_master.CollegeId == item.collegeId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            //  intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 6 && l.Labcode == "TMP-CL").Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 9)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 9 && l.CollegeId == item.collegeId && l.Year == 1 && l.Semester == 1).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 9 && l.jntuh_lab_master.CollegeId == item.collegeId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 9 && l.Labcode == "TMP-CL").Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 10)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 10 && l.CollegeId == item.collegeId).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 10 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            // intakedetails.Autonomus11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 510 && l.Labcode == "TMP-CL").Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }


                        intakedetails.requiredLabs12 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 2 && l.CollegeId == item.collegeId).Count();
                        intakedetails.uplodedLabs12 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 2 && l.jntuh_lab_master.CollegeId == item.collegeId).GroupBy(l => l.EquipmentID).Count();
                        //  intakedetails.Autonomus12 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 2 && l.Labcode == "TMP-CL").Count();
                        intakedetails.notuplodedLabs12 = intakedetails.requiredLabs12 - intakedetails.uplodedLabs12;

                        // intakedetailsList.Add(intakedetails);
                    }
                    else if ((CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null))
                    {
                        intakedetails.requiredLabs41 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 1 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs41 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs41 = intakedetails.requiredLabs41 - intakedetails.uplodedLabs41;

                        intakedetails.requiredLabs42 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 4 && l.Semester == 2 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs42 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 4 && l.jntuh_lab_master.Semester == 2).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs42 = intakedetails.requiredLabs42 - intakedetails.uplodedLabs42;

                        intakedetails.requiredLabs31 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 1 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs31 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs31 = intakedetails.requiredLabs31 - intakedetails.uplodedLabs31;

                        intakedetails.requiredLabs32 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 3 && l.Semester == 2 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs32 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 3 && l.jntuh_lab_master.Semester == 2).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs32 = intakedetails.requiredLabs32 - intakedetails.uplodedLabs32;

                        intakedetails.requiredLabs21 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 1 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs21 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs21 = intakedetails.requiredLabs21 - intakedetails.uplodedLabs21;

                        intakedetails.requiredLabs22 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 2 && l.Semester == 2 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs22 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 2 && l.jntuh_lab_master.Semester == 2).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs22 = intakedetails.requiredLabs22 - intakedetails.uplodedLabs22;


                        //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.CollegeId == null).Count();
                        //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count();
                        //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;

                        if (DegreeID == 1)
                        {
                            //intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.CollegeId == null || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 1 && l.SpecializationID == item.specializationId && l.CollegeId == null)).Count();  
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || (l.jntuh_lab_master.Year == 0 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.SpecializationID == item.specializationId)).GroupBy(l => l.EquipmentID).Count();
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 1 && l.Year == 1 && l.Semester == 1 && l.CollegeId == null).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 1 && l.jntuh_lab_master.CollegeId == null).GroupBy(l => l.EquipmentID).Count();
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 2)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 2 && l.Year == 1 && l.Semester == 1 && l.CollegeId == null || (l.Year == 0 && l.Semester == 0 && l.DegreeID == 2 && l.SpecializationID == item.specializationId && l.CollegeId == null)).Count(); // || (l.Year == 1 || l.Semester == 1)   && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || (l.jntuh_lab_master.Year == 0 && l.jntuh_lab_master.Semester == 0 && l.jntuh_lab_master.DegreeID == 2 && l.jntuh_lab_master.SpecializationID == item.specializationId)).GroupBy(l => l.EquipmentID).Count(); //|| (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)  && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 3)
                        {

                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 && l.DegreeID == 3 && l.Year == 1 && l.Semester == 1 && l.CollegeId == null).Count();
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 && l.jntuh_lab_master.DegreeID == 3).GroupBy(l => l.EquipmentID).Count();
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 4)
                        {
                            //if (Year == 1)
                            //{
                            //     intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1 || l.SpecializationID == 39).Count();
                            //intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            //else
                            //{
                            //    intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 1).Count();
                            //    intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 || l.jntuh_lab_master.SpecializationID == 39).Count();
                            //    intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                            //}
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == 39 && l.CollegeId == null).Count();  //l.SpecializationID == item.specializationId &&  l.Semester == 1 && 
                            //  intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.Year == 1 && l.SpecializationID == 39).Count();  //l.SpecializationID == item.specializationId &&  l.Semester == 1 && 
                            // intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.Year == 1  && l.jntuh_lab_master.SpecializationID == 39).GroupBy(l => l.EquipmentID).Count();   // && l.jntuh_lab_master.SpecializationID == item.specializationId  l.jntuh_lab_master.Semester == 1 &&
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == 39 && l.jntuh_lab_master.CollegeId == null).GroupBy(l => l.EquipmentID).Count();   // && l.jntuh_lab_master.SpecializationID == item.specializationId  l.jntuh_lab_master.Semester == 1 &&
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;

                        }
                        else if (DegreeID == 5)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 5 && l.CollegeId == null).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 5).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 6)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 6 && l.CollegeId == null && l.Year == 1 && l.Semester == 1).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 6 && l.jntuh_lab_master.CollegeId == null && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 9)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 9 && l.CollegeId == null).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 9).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }
                        else if (DegreeID == 10)
                        {
                            intakedetails.requiredLabs11 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.DegreeID == 10 && l.CollegeId == null).Count(); //   || (l.Year == 1 || l.Semester == 1)  && l.Year == 1 && l.Semester == 1
                            intakedetails.uplodedLabs11 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.DegreeID == 10).GroupBy(l => l.EquipmentID).Count(); //   || (l.jntuh_lab_master.Year == 0 || l.jntuh_lab_master.Semester == 0)     && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 1 
                            intakedetails.notuplodedLabs11 = intakedetails.requiredLabs11 - intakedetails.uplodedLabs11;
                        }


                        intakedetails.requiredLabs12 = jntuh_lab_master.Where(l => l.SpecializationID == item.specializationId && l.Year == 1 && l.Semester == 2 && l.CollegeId == null).Count();
                        intakedetails.uplodedLabs12 = jntuh_college_laboratories.Where(l => l.CollegeID == item.collegeId && l.jntuh_lab_master.SpecializationID == item.specializationId && l.jntuh_lab_master.Year == 1 && l.jntuh_lab_master.Semester == 2 && l.jntuh_lab_master.CollegeId == null).GroupBy(l => l.EquipmentID).Count();
                        intakedetails.notuplodedLabs12 = intakedetails.requiredLabs12 - intakedetails.uplodedLabs12;


                    }
                    intakedetailsList.Add(intakedetails);

                    #endregion



                }
                intakedetailsList = intakedetailsList.OrderBy(ei => ei.collegeName).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
                
            }
            return View("~/Views/Reports/LabsVideoVerificationAllColleges.cshtml", intakedetailsList);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AllCollegesIntakeWithVideoLabsDetails(int specializationId, int year, int semester, int collegeId, string type, int DegreeIds)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);
            string strdegree = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.jntuh_degree.degree).FirstOrDefault();
            string strdepartment = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.departmentName).FirstOrDefault();

            //string strcollegecode= db.jntuh_college_randamcodes.Find(collegeId).RandamCode;

            //jntuh_college_randamcodes randamCodes = new Models.jntuh_college_randamcodes();
            // randamCodes.RandamCode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault(); ;

            //if (strdegree != "B.Tech" && strdegree != "B.Pharmacy")
            //{
            //    year = 0;
            //    semester = 0;
            //}



            //int SpecializationId = 0;
            //  int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
            // int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
            //if (DegreeIds.Contains(4))
            //   SpecializationId = 39;
            // else
            //SpecializationId = 0;


            //int DegreeId = db.jntuh_lab_master.Where(M => M.SpecializationID == specializationId).Select(M=>M.DegreeID).FirstOrDefault();

            int DegreeId = DegreeIds;
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            List<jntuh_lab_master> collegeLabMaster = null;
            if (CollegeAffiliationStatus == "Yes")
            {
                if (DegreeId == 1)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 1 && l.CollegeId == collegeId).ToList(); // && l.Year == year && l.Semester == semester
                else if (DegreeId == 2)
                {
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.CollegeId == collegeId).ToList(); //  && l.Year == year && l.Semester == semester
                }

                else if (DegreeId == 3)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 3 && l.CollegeId == collegeId).ToList();
                else if (DegreeId == 4)
                {
                    if (year == 1 && semester == 1)
                    {
                        //collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l =>l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.CollegeId == collegeId && l.DegreeID == 4 || l.SpecializationID == 39).ToList();
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.CollegeId == collegeId && l.DegreeID == 4 && l.SpecializationID == 39).ToList();
                    }
                    else
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.CollegeId == collegeId).ToList();
                    }
                }
                else if (DegreeId == 5)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 5 && l.CollegeId == collegeId).ToList();  //  && l.Year == year && l.Semester == semester
                else if (DegreeId == 6)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 6 && l.CollegeId == collegeId).ToList();
                else if (DegreeId == 9)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 9 && l.CollegeId == collegeId).ToList();
                else if (DegreeId == 10)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 10 && l.CollegeId == collegeId).ToList();
            }
            else if ((CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null))
            {
                if (DegreeId == 1)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 1 && l.Year == year && l.Semester == semester && l.CollegeId == null).ToList(); //   && l.Year == year && l.Semester == semester 
                else if (DegreeId == 2)
                {
                    if (year == 1 && semester == 1)
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == year && l.Semester == semester && l.CollegeId == null || (l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == 0 && l.Semester == 0 && l.Labcode != "TMP-CL")).ToList();  // && l.Year == year && l.Semester == semester
                    else
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == year && l.Semester == semester && l.CollegeId == null).ToList();  // && l.Year == year && l.Semester == semester
                }

                else if (DegreeId == 3)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 3 && l.Year == year && l.Semester == semester && l.CollegeId == null).ToList();
                else if (DegreeId == 4)
                {
                    if (year == 1 && semester == 1)
                    {
                        //collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.CollegeId == null || l.SpecializationID == 39).ToList();
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == 39 && l.DegreeID == 4 && l.CollegeId == null).ToList();
                    }
                    else
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.CollegeId == null).ToList();
                    }
                }

                else if (DegreeId == 5)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.CollegeId == null && l.DegreeID == 5).ToList();  //  l.Semester == semester && l.DegreeID == 5 && && l.Year == year
                else if (DegreeId == 6)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.CollegeId == null && l.DegreeID == 6).ToList();
                else if (DegreeId == 9)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.CollegeId == null && l.DegreeID == 9).ToList();
                else if (DegreeId == 10)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.CollegeId == null && l.DegreeID == 10).ToList();
            }






            #region Committed by Suresh

            //if (DegreeId == 1)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 1  && l.Labcode != "TMP-CL").ToList(); //   && l.Year == year && l.Semester == semester 
            //else if (DegreeId == 2)
            //{
            //    if (year == 1 && semester == 1)
            //        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == year && l.Semester == semester && l.Labcode != "TMP-CL" || (l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == 0 && l.Semester == 0 && l.Labcode != "TMP-CL")).ToList();  // && l.Year == year && l.Semester == semester
            //    else
            //        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Year == year && l.Semester == semester && l.Labcode != "TMP-CL").ToList();  // && l.Year == year && l.Semester == semester
            //}

            //else if (DegreeId == 3)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 3 && l.Year == year && l.Semester == semester && l.Labcode != "TMP-CL").ToList();
            //else if (DegreeId == 4)
            //{

            //    if ((year == 1 && (semester == 1 || specializationId == 39)) || (year == 3 && semester == 0))
            //    {
            //        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.DegreeID == 4 && l.Labcode != "TMP-CL" && l.SpecializationID == 39).ToList();   //l.SpecializationID == specializationId &&  && l.Semester == semester
            //    }
            //    else
            //    {
            //        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.Labcode != "TMP-CL").ToList();

            //    }
            //}

            //else if (DegreeId == 5)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Labcode != "TMP-CL" && l.DegreeID == 5).ToList();  //  l.Semester == semester && l.DegreeID == 5 && && l.Year == year
            //else if (DegreeId == 6)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Labcode != "TMP-CL" && l.DegreeID == 6).ToList();
            //else if (DegreeId == 9)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Labcode != "TMP-CL" && l.DegreeID == 9).ToList();
            //else if (DegreeId == 10)
            //    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Labcode != "TMP-CL" && l.DegreeID == 10).ToList();

            #endregion

            var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();


            #region Old Code Commented By Srinivas
            //foreach (var item in collegeLabMaster)
            //{
            //    if (item.jntuh_degree.degree != "B.Tech" && item.jntuh_degree.degree != "B.Pharmacy")
            //    {
            //        for (int i = 1; i <= PGEquipmentCount; i++)
            //        {
            //            Lab lstlabs = new Lab();
            //            lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
            //            lstlabs.EquipmentID = item.id;
            //            lstlabs.degree = item.jntuh_degree.degree;
            //            lstlabs.department = item.jntuh_department.departmentName;
            //            lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //            lstlabs.Semester = item.Semester;
            //            lstlabs.year = item.Year;
            //            lstlabs.Labcode = item.Labcode;
            //            lstlabs.LabName = item.LabName;
            //            lstlabs.EquipmentName = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == i && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
            //            lstlabs.LabEquipmentName = item.EquipmentName;
            //            lstlabs.collegeId = collegeId;
            //            lstlabs.EquipmentNo = i;
            //            lstlabs.RandomCode = strcollegecode;
            //            lstlaboratories.Add(lstlabs);
            //        }
            //    }
            //    else
            //    {
            //        Lab lstlabs = new Lab();
            //        lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
            //        lstlabs.EquipmentID = item.id;
            //        lstlabs.degree = item.jntuh_degree.degree;
            //        lstlabs.department = item.jntuh_department.departmentName;
            //        lstlabs.specializationName = item.jntuh_specialization.specializationName;
            //        lstlabs.Semester = item.Semester;
            //        lstlabs.year = item.Year;
            //        lstlabs.Labcode = item.Labcode;
            //        lstlabs.LabName = item.LabName;
            //        lstlabs.EquipmentName = item.EquipmentName;
            //        lstlabs.LabEquipmentName = item.EquipmentName;
            //        lstlabs.collegeId = collegeId;
            //        lstlabs.EquipmentNo = 1;
            //        lstlabs.RandomCode = strcollegecode;
            //        lstlaboratories.Add(lstlabs);
            //    }
            //}
            #endregion

            #region New Code Written By Srinivas
            if (collegeLabMaster != null)
            {
                foreach (var item in collegeLabMaster)
                {

                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                    lstlabs.IsActive = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.isActive).FirstOrDefault()??false;
                    lstlabs.EquipmentID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName = !string.IsNullOrEmpty(item.LabName) ? item.LabName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.LabName).FirstOrDefault();
                    lstlabs.EquipmentName = !string.IsNullOrEmpty(item.EquipmentName) ? item.EquipmentName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
                    lstlabs.LabEquipmentName = !string.IsNullOrEmpty(item.EquipmentName) ? item.EquipmentName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
                    lstlabs.collegeId = collegeId;
                    lstlabs.EquipmentNo = 1;
                    lstlabs.EquipmentDateOfPurchasing =jntuh_college_laboratories.Where(L => L.EquipmentID == item.id && L.CollegeID == collegeId).Select(L => L.EquipmentDateOfPurchasing).FirstOrDefault();
                    lstlabs.RandomCode = strcollegecode;
                    lstlabs.updatedBy = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.updatedBy).FirstOrDefault();
                    lstlabs.updatedOn = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.updatedOn).FirstOrDefault();
                    lstlabs.createdOn = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.createdOn).FirstOrDefault();
                    lstlaboratories.Add(lstlabs);





                }
            }

            #endregion



            return View("~/Views/Reports/AllCollegesIntakeWithVideoLabsDetails.cshtml", lstlaboratories);
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AutonomusCollegesIntakeWithVideoLabsDetails(int specializationId, int year, int semester, int collegeId, string type, int DegreeIds)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            int PGEquipmentCount = Convert.ToInt32(WebConfigurationManager.AppSettings["PGEquipmentCount"]);
            string strdegree = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.jntuh_degree.degree).FirstOrDefault();
            string strdepartment = db.jntuh_specialization.Where(s => s.id == specializationId).Select(s => s.jntuh_department.departmentName).FirstOrDefault();

            //string strcollegecode= db.jntuh_college_randamcodes.Find(collegeId).RandamCode;

            //jntuh_college_randamcodes randamCodes = new Models.jntuh_college_randamcodes();
            // randamCodes.RandamCode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeId).Select(r => r.RandamCode).FirstOrDefault(); ;

            //if (strdegree != "B.Tech" && strdegree != "B.Pharmacy")
            //{
            //    year = 0;
            //    semester = 0;
            //}



            //int SpecializationId = 0;
            //  int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
            // int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
            //if (DegreeIds.Contains(4))
            //   SpecializationId = 39;
            // else
            //SpecializationId = 0;
            int DegreeId = DegreeIds;
            //int DegreeId = db.jntuh_lab_master.Where(M => M.SpecializationID == specializationId).Select(M => M.DegreeID).FirstOrDefault();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            List<jntuh_lab_master> collegeLabMaster = null;
            if (CollegeAffiliationStatus == "Yes")
            {
                if (DegreeId == 1)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 1 && l.Labcode == "TMP-CL").ToList(); // && l.Year == year && l.Semester == semester
                else if (DegreeId == 2)
                {
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 2 && l.Labcode == "TMP-CL").ToList(); //  && l.Year == year && l.Semester == semester
                }

                else if (DegreeId == 3)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 3 && l.Labcode == "TMP-CL").ToList();
                else if (DegreeId == 4)
                {
                    if (year == 1 && semester == 1)
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.Year == 0 && l.Semester == 0 && l.DegreeID == 4 && l.Labcode == "TMP-CL" && l.SpecializationID == specializationId).ToList();
                    }
                    else
                    {
                        collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.Year == year && l.Semester == semester && l.DegreeID == 4 && l.Labcode == "TMP-CL").ToList();
                    }
                }
                else if (DegreeId == 5)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 5 && l.Labcode == "TMP-CL").ToList();  //  && l.Year == year && l.Semester == semester
                else if (DegreeId == 6)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 6 && l.Labcode == "TMP-CL").ToList();
                else if (DegreeId == 9)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 9 && l.Labcode == "TMP-CL").ToList();
                else if (DegreeId == 10)
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => l.SpecializationID == specializationId && l.DegreeID == 10 && l.Labcode == "TMP-CL").ToList();
            }



            var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeId).ToList();

            #region New Code Written By Srinivas
            if (collegeLabMaster != null)
            {
                foreach (var item in collegeLabMaster)
                {

                    Lab lstlabs = new Lab();
                    lstlabs.id = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.id).FirstOrDefault();
                    lstlabs.EquipmentID = item.id;
                    lstlabs.degree = item.jntuh_degree.degree;
                    lstlabs.department = item.jntuh_department.departmentName;
                    lstlabs.specializationName = item.jntuh_specialization.specializationName;
                    lstlabs.Semester = item.Semester;
                    lstlabs.year = item.Year;
                    lstlabs.Labcode = item.Labcode;
                    lstlabs.LabName = !string.IsNullOrEmpty(item.LabName) ? item.LabName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.LabName).FirstOrDefault();
                    lstlabs.EquipmentName = !string.IsNullOrEmpty(item.EquipmentName) ? item.EquipmentName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
                    lstlabs.LabEquipmentName = !string.IsNullOrEmpty(item.EquipmentName) ? item.EquipmentName : jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.EquipmentNo == 1 && l.CollegeID == collegeId).Select(l => l.EquipmentName).FirstOrDefault();
                    lstlabs.collegeId = collegeId;
                    lstlabs.EquipmentNo = 1;
                    lstlabs.RandomCode = strcollegecode;
                    lstlabs.updatedBy = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.updatedBy).FirstOrDefault();
                    lstlabs.updatedOn = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.updatedOn).FirstOrDefault();
                    lstlabs.createdOn = jntuh_college_laboratories.Where(l => l.EquipmentID == item.id && l.CollegeID == collegeId).Select(l => l.createdOn).FirstOrDefault();
                    lstlaboratories.Add(lstlabs);
                }
            }
            #endregion



            return View("~/Views/Reports/AutonomusCollegesIntakeWithVideoLabsDetails", lstlaboratories);
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College,DataEntry")]
        public ActionResult ActiveRecord(int id)
        {

            var lab = db.jntuh_college_laboratories.Where(l => l.id == id).Select(l => l).FirstOrDefault();
            int collegeId = lab.CollegeID;
            var labmaster = db.jntuh_lab_master.Where(l => l.id == lab.EquipmentID).Select(l => l).FirstOrDefault();
            if (lab != null)
            {
                try
                {
                    lab.isActive = true;
                    db.Entry(lab).State=EntityState.Modified;
                    
                    db.SaveChanges();
                    TempData["Success"] = "Lab Related details Activated Successfully.";
                }
                catch { }
            }
            if (labmaster.SpecializationID == 39)
            {
                return RedirectToAction("AllCollegesIntakeWithVideoLabsDetails", new { specializationId = labmaster.SpecializationID, year = labmaster.Year, semester = 1, collegeId, DegreeIds = labmaster.DegreeID });
            }
            else
            {
                return RedirectToAction("AllCollegesIntakeWithVideoLabsDetails", new { specializationId = labmaster.SpecializationID, year = labmaster.Year, semester = labmaster.Semester, collegeId, DegreeIds = labmaster.DegreeID });
            }



        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College,DataEntry")]
        public ActionResult InActiveRecord(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var lab = db.jntuh_college_laboratories.Where(l => l.id == id).Select(l => l).FirstOrDefault();
            int collegeId = lab.CollegeID;
            var labmaster = db.jntuh_lab_master.Where(l => l.id == lab.EquipmentID).Select(l => l).FirstOrDefault();
            if (lab != null)
            {
                try
                {
                    lab.isActive = false;
                    lab.updatedBy = userID;
                    lab.updatedOn = DateTime.Now;
                    db.Entry(lab).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Lab Related details In Activated Successfully.";
                }
                catch { }
            }
            if (labmaster.SpecializationID == 39)
            {
                return RedirectToAction("AllCollegesIntakeWithVideoLabsDetails", new { specializationId = labmaster.SpecializationID, year = labmaster.Year, semester = 1, collegeId, DegreeIds = labmaster.DegreeID });
            }
            else
            {
                return RedirectToAction("AllCollegesIntakeWithVideoLabsDetails", new { specializationId = labmaster.SpecializationID, year = labmaster.Year, semester = labmaster.Semester, collegeId, DegreeIds = labmaster.DegreeID });
            }



        }

    }
}
