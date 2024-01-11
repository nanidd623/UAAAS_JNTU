using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;
using System.Data;
namespace UAAAS.Controllers.Admin
{
    [ErrorHandling]
    public class AuditorWiseReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AuditorWiseReport()
        {
            var auditorsList = (from ffc in db.jntuh_ffc_committee
                                join s in db.jntuh_ffc_schedule on ffc.scheduleID equals s.id
                                join a in db.jntuh_ffc_auditor on ffc.auditorID equals a.id
                                join c in db.jntuh_college on s.collegeID equals c.id
                                where (c.isActive == true && a.isActive == true)
                                orderby (s.inspectionDate) descending
                                select new
                                {
                                    ffc.auditorID,
                                    a.auditorName,
                                    a.auditorPreferredDesignation,
                                    a.auditorPlace,
                                    s.collegeID,
                                    c.collegeCode,
                                    c.collegeName,
                                    inspectionDate = s.inspectionDate
                                }).ToList();


            List<AuditorWiseReport> auditorWiseReport = new List<Models.AuditorWiseReport>();
            if (auditorsList != null)
            {

                foreach (var item in auditorsList)
                {
                    AuditorWiseReport auditors = new Models.AuditorWiseReport();
                    auditors.auditorID = item.auditorID;
                    auditors.auditorName = item.auditorName;
                    auditors.auditorPreferredDesignation = item.auditorPreferredDesignation;
                    auditors.auditorPlace = item.auditorPlace;
                    auditors.collegeID = item.collegeID;
                    auditors.collegeCode = item.collegeCode;
                    auditors.collegeName = item.collegeName;
                    if (item.inspectionDate != null)
                    {
                        auditors.strinspectionDate = Utilities.MMDDYY2DDMMYY(item.inspectionDate.ToString()); ;
                    }
                    else
                    {
                        auditors.strinspectionDate = "";
                    }
                    auditorWiseReport.Add(auditors);
                }

            }


            return View("~/Views/Admin/AuditorWiseReport.cshtml", auditorWiseReport);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ExportData()
        {
            var auditorsList = (from ffc in db.jntuh_ffc_committee
                                join s in db.jntuh_ffc_schedule on ffc.scheduleID equals s.id
                                join a in db.jntuh_ffc_auditor on ffc.auditorID equals a.id
                                join c in db.jntuh_college on s.collegeID equals c.id
                                where (c.isActive == true && a.isActive == true)
                                orderby (s.inspectionDate) descending
                                select new
                                {
                                    ffc.auditorID,
                                    a.auditorName,
                                    a.auditorPreferredDesignation,
                                    a.auditorPlace,
                                    s.collegeID,
                                    c.collegeCode,
                                    c.collegeName,
                                    inspectionDate = s.inspectionDate
                                }).ToList();
            GridView gv = new GridView();  

            DataTable dt = new DataTable();
            dt.Columns.Add("S No", typeof(string));
            dt.Columns.Add("Auditor Name", typeof(string));
            dt.Columns.Add("Designation", typeof(string));
            dt.Columns.Add("Place", typeof(string));
            dt.Columns.Add("College Code", typeof(string));
            dt.Columns.Add("College Name", typeof(string));
            dt.Columns.Add("Inspection Date", typeof(string));
            int i = 1;
            foreach (var item in auditorsList)
            {
                DataRow dr = dt.NewRow();
                dr["S No"] = i;
                dr["Auditor Name"] = item.auditorName;
                dr["Designation"] = item.auditorPreferredDesignation;
                dr["Place"] = item.auditorPlace;
                dr["College Code"] = item.collegeCode;
                dr["College Name"] = item.collegeName;
                if (item.inspectionDate != null)
                {
                    dr["Inspection Date"] = Utilities.MMDDYY2DDMMYY(item.inspectionDate.ToString());
                }
                else
                {
                    dr["Inspection Date"] = string.Empty;
                }
                i++;
                dt.Rows.Add(dr);
            }

            gv.DataSource = dt;

            gv.HeaderStyle.BackColor = System.Drawing.Color.FromName("#6495ED");
            gv.HeaderStyle.Font.Bold = true;
            gv.HeaderStyle.ForeColor = System.Drawing.Color.White;
            gv.RowStyle.BackColor = System.Drawing.Color.FromName("#A9A9A9");
            gv.AlternatingRowStyle.BackColor = System.Drawing.Color.Silver;
            gv.DataBind();           

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=AuditorsWiseReport.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            
            List<AuditorWiseReport> auditorWiseReport = new List<Models.AuditorWiseReport>();
            if (auditorsList != null)
            {

                foreach (var item in auditorsList)
                {
                    AuditorWiseReport auditors = new Models.AuditorWiseReport();
                    auditors.auditorID = item.auditorID;
                    auditors.auditorName = item.auditorName;
                    auditors.auditorPreferredDesignation = item.auditorPreferredDesignation;
                    auditors.auditorPlace = item.auditorPlace;
                    auditors.collegeID = item.collegeID;
                    auditors.collegeCode = item.collegeCode;
                    auditors.collegeName = item.collegeName;
                    if (item.inspectionDate != null)
                    {
                        auditors.strinspectionDate = Utilities.MMDDYY2DDMMYY(item.inspectionDate.ToString()); ;
                    }
                    else
                    {
                        auditors.strinspectionDate = "";
                    }
                    auditorWiseReport.Add(auditors);
                }

            }


            return View("~/Views/Admin/AuditorWiseReport.cshtml", auditorWiseReport);

         

            //return RedirectToAction("AuditorWiseReport");

        }     
        

    }
}
