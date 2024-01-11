using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace UAAAS.Mailers
{
    public class LogController : Controller
    {
        static string filepath = "";
        FileStream fs;
        StreamWriter log;

        public void Logger()
	{

		//
		// TODO: Add constructor logic here
		//


        string LogFileDir= Server.MapPath(".").ToString() + "\\LogDir";
       // string logpath = HttpContext.Current.Request.Url.AbsolutePath.ToString();

        if (!Directory.Exists(LogFileDir))
        {
            Directory.CreateDirectory(LogFileDir);
        }
        filepath = LogFileDir + "\\" + @"LogFile" + DateTime.Now.ToString("CL_yyyyMMdd") + ".txt";
     
	}

    public void Log(string ErrorMessage)
    {

        FileStream fs;
        StreamWriter log;
        string filepath = "";
        //string LogFileDir = "~/Content/Upload/LogDir";
        string LogFileDir = HostingEnvironment.MapPath("~/LogDir");
        //string LogFileDir = System.Web.HttpContext.Current.Server.MapPath(".").ToString() + "\\LogDir";
        string message = "";
        //message = regFaculty.RegistrationNumber + "<br/>," + regFaculty.Email+"<br/>"+ex.Message;
        message = ErrorMessage;
        if (!Directory.Exists(LogFileDir))
        {
            Directory.CreateDirectory(LogFileDir);
        }
        filepath = LogFileDir + "\\" + @"LogFile" + DateTime.Now.ToString("CL_yyyyMMdd") + ".txt";
        fs = new FileStream(filepath, FileMode.Append);
        log = new StreamWriter(fs, Encoding.UTF8);
        log.WriteLine("Date : {0} Time : {1}", DateTime.Now.ToString("MMMM dd, yyyy"), DateTime.Now.ToString("hh:mm:ss"));
        log.WriteLine("Message : {0} ", message);
        log.Close();
        fs.Close();

    }

    }
}
