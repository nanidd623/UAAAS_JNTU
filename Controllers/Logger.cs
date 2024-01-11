using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text;
using System.Web;

namespace PANAPIMVC.Controllers
{
    public class Logger
    {
        static string filepath = "";
        FileStream fs;
        StreamWriter log;
        public Logger()
        {

            //
            // TODO: Add constructor logic here
            //


            string LogFileDir = HttpContext.Current.Server.MapPath(".").ToString() + "\\LogDir";
            string logpath = HttpContext.Current.Request.Url.AbsolutePath.ToString();

            if (!Directory.Exists(LogFileDir))
            {
                Directory.CreateDirectory(LogFileDir);
            }
            filepath = LogFileDir + "\\" + @"LogFile" + DateTime.Now.ToString("CL_yyyyMMdd") + ".txt";

        }

        public void Log(string message)
        {

            fs = new FileStream(filepath, FileMode.Append);
            log = new StreamWriter(fs, Encoding.UTF8);
            log.WriteLine("Date : {0} Time : {1}", DateTime.Now.ToString("MMMM dd, yyyy"), DateTime.Now.ToString("hh:mm:ss"));
            log.WriteLine("Message : {0} ", message);
            log.Close();
            fs.Close();

        }
    }
}
