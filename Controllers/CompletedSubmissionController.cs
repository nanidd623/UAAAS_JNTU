using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CompletedSubmissionController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /PendingSubmission/

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            DateTime Updatedate = new DateTime(2018, 01, 31, 00, 00, 00);
            List<CompletedSubmission> completedSubmissionColleges = new List<CompletedSubmission>();
            var actualYear =
                db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            var AcademicYearId =
                db.jntuh_academic_year.Where(d => d.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();
            //int[] SubmittedCollegeIds = { 2, 4, 7, 8, 9, 11, 12, 17, 20, 22, 23, 26, 29, 32, 34, 38, 40, 41, 46, 48, 56, 59, 68, 69, 70, 72, 74, 77, 79, 80, 81, 84, 85, 86, 87, 88, 100, 102, 103, 104, 106, 108, 109, 111, 113, 115, 116, 119, 121, 122, 123, 124, 125, 128, 129, 130, 132, 134, 137, 138, 141, 143, 144, 145, 147, 148, 151, 152, 153, 155, 156, 157, 158, 159, 161, 162, 163, 164, 165, 166, 168, 170, 171, 172, 173, 175, 176, 177, 178, 179, 181, 182, 183, 184, 185, 186, 187, 188, 189, 192, 193, 195, 196, 197, 198, 201, 203, 207, 210, 211, 214, 215, 218, 222, 225, 227, 228, 229, 236, 238, 241, 242, 243, 244, 245, 247, 249, 250, 254, 256, 259, 260, 261, 264, 269, 271, 273, 276, 282, 283, 286, 287, 291, 292, 293, 299, 300, 304, 305, 306, 307, 308, 309, 310, 315, 316, 321, 322, 324, 326, 327, 329, 330, 334, 335, 336, 342, 349, 350, 352, 360, 365, 366, 367, 368, 369, 371, 373, 374, 376, 380, 382, 385, 391, 393, 394, 395, 399, 400, 401, 402, 403, 414, 415, 416, 419, 420, 422, 423, 424, 428, 429, 430, 6, 24, 27, 30, 44, 45, 47, 52, 54, 55, 58, 60, 65, 66, 78, 90, 95, 97, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 169, 202, 204, 206, 213, 219, 234, 237, 239, 252, 253, 262, 263, 267, 290, 295, 297, 298, 301, 302, 303, 313, 314, 317, 318, 319, 320, 348, 353, 362, 370, 379, 384, 389, 392, 410, 427, 5, 67, 246, 279, 296, 325, 343, 355, 386, 411, 421, 39, 42, 43, 75, 140, 180, 194, 217, 223, 230, 235, 266, 332, 364, 35, 50, 91, 174, 435, 436, 439, 441, 442, 443, 445, 447, 448, 452, 454, 455, 413, 449 };
            int[] IsEditableColleges = db.jntuh_college_edit_status.Where(s => s.academicyearId == AcademicYearId && s.IsCollegeEditable == false).Select(s => s.collegeId).Distinct().ToArray(); // && SubmittedCollegeIds.Contains(s.collegeId)

            //int[] IsEditableColleges = db.jntuh_college_edit_status.Where(editStatus => SubmittedCollegeIds.Contains(editStatus.collegeId))
            //                                                    .Select(editStatus => editStatus.collegeId)
            //                                                    .ToArray();
            var jntuh_colleges = db.jntuh_college.Where(s => s.isActive == true && IsEditableColleges.Contains(s.id)).ToList();//
            var jntuh_college_edit_statuss = db.jntuh_college_edit_status.ToList();
            var Collegetype = db.jntuh_college_type.Where(c => c.isActive == true).ToList();
            var CollegeAddress = db.jntuh_address.AsNoTracking().ToList();
            var Districts = db.jntuh_district.Where(c => c.isActive == true).ToList();
            foreach (var collegeId in jntuh_colleges)
            {
                if (collegeId.id != 375)
                {
                    var completedSubmissionCollege = new CompletedSubmission();
                    completedSubmissionCollege.collegeId = collegeId.id;
                    completedSubmissionCollege.collegeCode = jntuh_colleges.Where(editableCollege => editableCollege.id == collegeId.id)
                                                                           .Select(editableCollege => editableCollege.collegeCode)
                                                                           .FirstOrDefault();
                    completedSubmissionCollege.collegeName = jntuh_colleges.Where(editableCollege => editableCollege.id == collegeId.id)
                                                                           .Select(editableCollege => editableCollege.collegeName)
                                                                           .FirstOrDefault();
                    //completedSubmissionCollege.submittedDate = jntuh_colleges.Where(editableCollege => editableCollege.id == collegeId.id && editableCollege.updatedOn >= Updatedate)
                    //                                                      .Select(editableCollege => editableCollege.updatedOn)
                    //                                                      .FirstOrDefault();
                    var CollegeAdd = CollegeAddress.Where(a => a.collegeId == collegeId.id).FirstOrDefault();
                    if (CollegeAdd != null)
                    {
                        completedSubmissionCollege.district = Districts.Where(i => i.id == CollegeAdd.districtId).FirstOrDefault().districtName;
                    }
                    completedSubmissionCollege.Collegetype = Collegetype.Where(c => c.id == collegeId.collegeTypeID).Select(c => c.collegeType).FirstOrDefault();
                    completedSubmissionCollege.submittedDate = jntuh_college_edit_statuss.Where(submitDate => submitDate.collegeId == collegeId.id && submitDate.academicyearId == AcademicYearId)
                                                                                           .Select(submitDate => submitDate.updatedOn)
                                                                                           .FirstOrDefault();
                    if (completedSubmissionCollege.submittedDate != null)
                    {
                        completedSubmissionCollege.submitdate = Utilities.MMDDYY2DDMMYY(completedSubmissionCollege.submittedDate.ToString());
                    }
                    else
                    {
                        completedSubmissionCollege.submitdate = string.Empty;
                    }
                    completedSubmissionColleges.Add(completedSubmissionCollege);
                }
            }
            ViewBag.Colleges = completedSubmissionColleges;
            ViewBag.Count = completedSubmissionColleges.Count();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Index(CompletedSubmission completedSubmission)
        {
            List<CompletedSubmission> completedSubmissionColleges = new List<CompletedSubmission>();
            var actualYear =
               db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
                   .Select(a => a.actualYear)
                   .FirstOrDefault();
            var AcademicYearId =
                db.jntuh_academic_year.Where(d => d.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();
            if (completedSubmission.collegeCode != null)
            {


                int[] id = db.jntuh_college.Where(college => college.collegeCode == completedSubmission.collegeCode.Trim())
                                                  .Select(college => college.id)
                                                  .ToArray();
                foreach (var collegeId in id)
                {
                    bool isEditStatus = db.jntuh_college_edit_status.Where(isEditableStatus => isEditableStatus.academicyearId == AcademicYearId && isEditableStatus.collegeId == collegeId).Select(isEditableStatus => isEditableStatus.IsCollegeEditable).FirstOrDefault();
                    if (isEditStatus == false)
                    {
                        CompletedSubmission completedSubmissionCollege = new CompletedSubmission();
                        completedSubmissionCollege.collegeId = collegeId;
                        completedSubmissionCollege.collegeCode = db.jntuh_college.Where(editableCollege => editableCollege.id == collegeId)
                                                                               .Select(editableCollege => editableCollege.collegeCode)
                                                                               .FirstOrDefault();
                        completedSubmissionCollege.collegeName = db.jntuh_college.Where(editableCollege => editableCollege.id == collegeId)
                                                                               .Select(editableCollege => editableCollege.collegeName)
                                                                               .FirstOrDefault();
                        completedSubmissionCollege.submittedDate = db.jntuh_college_edit_status.Where(editableCollege => editableCollege.academicyearId == AcademicYearId && editableCollege.collegeId == collegeId)
                                                                                               .Select(editableCollege => editableCollege.updatedOn)
                                                                                               .FirstOrDefault();
                        if (completedSubmissionCollege.submittedDate != null)
                        {
                            completedSubmissionCollege.submitdate = Utilities.MMDDYY2DDMMYY(completedSubmissionCollege.submittedDate.ToString());
                        }
                        else
                        {
                            completedSubmissionCollege.submitdate = string.Empty;
                        }
                        completedSubmissionColleges.Add(completedSubmissionCollege);
                    }
                }
            }
            if (completedSubmission.collegeName != null)
            {
                int[] id = db.jntuh_college.Where(college => college.collegeName == completedSubmission.collegeName.Trim())
                                                  .Select(college => college.id)
                                                  .ToArray();
                foreach (var collegeId in id)
                {
                    bool isEditStatus = db.jntuh_college_edit_status.Where(isEditableStatus => isEditableStatus.academicyearId == AcademicYearId && isEditableStatus.collegeId == collegeId).Select(isEditableStatus => isEditableStatus.IsCollegeEditable).FirstOrDefault();
                    if (isEditStatus == false)
                    {
                        CompletedSubmission completedSubmissionCollege = new CompletedSubmission();
                        completedSubmissionCollege.collegeId = collegeId;
                        completedSubmissionCollege.collegeCode = db.jntuh_college.Where(editableCollege => editableCollege.id == collegeId)
                                                                               .Select(editableCollege => editableCollege.collegeCode)
                                                                               .FirstOrDefault();
                        completedSubmissionCollege.collegeName = db.jntuh_college.Where(editableCollege => editableCollege.id == collegeId)
                                                                               .Select(editableCollege => editableCollege.collegeName)
                                                                               .FirstOrDefault();
                        completedSubmissionCollege.submittedDate = db.jntuh_college_edit_status.Where(editableCollege => editableCollege.academicyearId == AcademicYearId && editableCollege.collegeId == collegeId)
                                                                                               .Select(editableCollege => editableCollege.updatedOn)
                                                                                               .FirstOrDefault();
                        if (completedSubmissionCollege.submittedDate != null)
                        {
                            completedSubmissionCollege.submitdate = Utilities.MMDDYY2DDMMYY(completedSubmissionCollege.submittedDate.ToString());
                        }
                        else
                        {
                            completedSubmissionCollege.submitdate = string.Empty;
                        }
                        completedSubmissionColleges.Add(completedSubmissionCollege);
                    }
                }
            }

            ViewBag.Colleges = completedSubmissionColleges;
            ViewBag.Count = completedSubmissionColleges.Count();
            return View("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ExcelCompletedSubmission()
        {
            DateTime Updatedate = new DateTime(2018, 01, 31, 00, 00, 00);
            List<CompletedSubmission> completedSubmissionColleges = new List<CompletedSubmission>();
            var actualYear =
                db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            var AcademicYearId =
                db.jntuh_academic_year.Where(d => d.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();
            //int[] SubmittedCollegeIds = { 2, 4, 7, 8, 9, 11, 12, 17, 20, 22, 23, 26, 29, 32, 34, 38, 40, 41, 46, 48, 56, 59, 68, 69, 70, 72, 74, 77, 79, 80, 81, 84, 85, 86, 87, 88, 100, 102, 103, 104, 106, 108, 109, 111, 113, 115, 116, 119, 121, 122, 123, 124, 125, 128, 129, 130, 132, 134, 137, 138, 141, 143, 144, 145, 147, 148, 151, 152, 153, 155, 156, 157, 158, 159, 161, 162, 163, 164, 165, 166, 168, 170, 171, 172, 173, 175, 176, 177, 178, 179, 181, 182, 183, 184, 185, 186, 187, 188, 189, 192, 193, 195, 196, 197, 198, 201, 203, 207, 210, 211, 214, 215, 218, 222, 225, 227, 228, 229, 236, 238, 241, 242, 243, 244, 245, 247, 249, 250, 254, 256, 259, 260, 261, 264, 269, 271, 273, 276, 282, 283, 286, 287, 291, 292, 293, 299, 300, 304, 305, 306, 307, 308, 309, 310, 315, 316, 321, 322, 324, 326, 327, 329, 330, 334, 335, 336, 342, 349, 350, 352, 360, 365, 366, 367, 368, 369, 371, 373, 374, 376, 380, 382, 385, 391, 393, 394, 395, 399, 400, 401, 402, 403, 414, 415, 416, 419, 420, 422, 423, 424, 428, 429, 430, 6, 24, 27, 30, 44, 45, 47, 52, 54, 55, 58, 60, 65, 66, 78, 90, 95, 97, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 169, 202, 204, 206, 213, 219, 234, 237, 239, 252, 253, 262, 263, 267, 290, 295, 297, 298, 301, 302, 303, 313, 314, 317, 318, 319, 320, 348, 353, 362, 370, 379, 384, 389, 392, 410, 427, 5, 67, 246, 279, 296, 325, 343, 355, 386, 411, 421, 39, 42, 43, 75, 140, 180, 194, 217, 223, 230, 235, 266, 332, 364, 35, 50, 91, 174, 435, 436, 439, 441, 442, 443, 445, 447, 448, 452, 454, 455, 413, 449 };
            int[] IsEditableColleges = db.jntuh_college_edit_status.Where(s => s.academicyearId == AcademicYearId && s.IsCollegeEditable == false).Select(s => s.collegeId).Distinct().ToArray(); // && SubmittedCollegeIds.Contains(s.collegeId)

            //int[] IsEditableColleges = db.jntuh_college_edit_status.Where(editStatus => SubmittedCollegeIds.Contains(editStatus.collegeId))
            //                                                    .Select(editStatus => editStatus.collegeId)
            //                                                    .ToArray();
            var jntuh_colleges = db.jntuh_college.Where(s => s.isActive == true && IsEditableColleges.Contains(s.id)).ToList();//
            var jntuh_college_edit_statuss = db.jntuh_college_edit_status.ToList();
            var Collegetype = db.jntuh_college_type.Where(c => c.isActive == true).ToList();
            var CollegeAddress = db.jntuh_address.AsNoTracking().ToList();
            var Districts = db.jntuh_district.Where(c => c.isActive == true).ToList();
            foreach (var collegeId in jntuh_colleges)
            {
                if (collegeId.id != 375)
                {
                    var completedSubmissionCollege = new CompletedSubmission();
                    completedSubmissionCollege.collegeId = collegeId.id;
                    completedSubmissionCollege.collegeCode = jntuh_colleges.Where(editableCollege => editableCollege.id == collegeId.id)
                                                                           .Select(editableCollege => editableCollege.collegeCode)
                                                                           .FirstOrDefault();
                    completedSubmissionCollege.collegeName = jntuh_colleges.Where(editableCollege => editableCollege.id == collegeId.id)
                                                                           .Select(editableCollege => editableCollege.collegeName)
                                                                           .FirstOrDefault();
                    //completedSubmissionCollege.submittedDate = jntuh_colleges.Where(editableCollege => editableCollege.id == collegeId.id && editableCollege.updatedOn >= Updatedate)
                    //                                                      .Select(editableCollege => editableCollege.updatedOn)
                    //                                                      .FirstOrDefault();
                    var CollegeAdd = CollegeAddress.Where(a => a.collegeId == collegeId.id).FirstOrDefault();
                    if (CollegeAdd != null)
                    {
                        completedSubmissionCollege.district = Districts.Where(i => i.id == CollegeAdd.districtId).FirstOrDefault().districtName;
                    }
                    completedSubmissionCollege.Collegetype = Collegetype.Where(c => c.id == collegeId.collegeTypeID).Select(c => c.collegeType).FirstOrDefault();
                    completedSubmissionCollege.submittedDate = jntuh_college_edit_statuss.Where(submitDate => submitDate.collegeId == collegeId.id && submitDate.academicyearId == AcademicYearId)
                                                                                           .Select(submitDate => submitDate.updatedOn)
                                                                                           .FirstOrDefault();
                    if (completedSubmissionCollege.submittedDate != null)
                    {
                        completedSubmissionCollege.submitdate = Utilities.MMDDYY2DDMMYY(completedSubmissionCollege.submittedDate.ToString());
                    }
                    else
                    {
                        completedSubmissionCollege.submitdate = string.Empty;
                    }
                    completedSubmissionColleges.Add(completedSubmissionCollege);
                }
            }
            ViewBag.Colleges = completedSubmissionColleges;
            ViewBag.Count = completedSubmissionColleges.Count();

            string ReportHeader = "completedSubmissionCollegeslist.xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/CompletedSubmission/CollegeCompletedSubmissionlistExport.cshtml");
        }
    }
}
