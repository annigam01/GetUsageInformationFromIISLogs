using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace GetUsageInformationFromIISLogs
{
    class Helper
    {
        public static void Log(string Message, bool ShowInUI = true) {

            if (Program.isLoggingOn)
            {
                System.IO.File.AppendAllText(Program.GlobalLogFilepath, DateTime.Now.ToLongTimeString() + ": " + Message + Environment.NewLine);
                
            }
            if (ShowInUI)
            {
                Console.WriteLine(Message);
            }

        }
        public static string SetFilePath()
        {

            //returns the file path to be used thruout the execution

            string filename = "";

            string currentDate = @"\Date- " + DateTime.Today.ToShortDateString().Replace('/', '_');

            string currentTime = " Time-" + filename + DateTime.Now.ToShortTimeString().Replace(' ', '-').Replace(':', '-');

            //filepath
            string extension = ".txt";

            //read from config the log folder path
            string LogDirectory = ConfigurationManager.AppSettings["LogDirectory"];

            //construct the file path with file name at the end
            filename = currentDate + currentTime + extension;

            string filepath = LogDirectory + filename;

            return filepath;
        }
        public static bool SetLogging()
        {
            return bool.Parse(ConfigurationManager.AppSettings["isLoggingOn"]);
        }
        public static SecureString ConvertToSecureString(string strPassword)
        {
            var secureStr = new SecureString();
            if (strPassword.Length > 0)
            {
                foreach (var c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            }
            return secureStr;

        }
        public static void LogWeb(string message)
        {
            System.IO.File.AppendAllText(Program.SPWeboutputFileWithPath, message+Environment.NewLine);
        }
        public static void ClearFileContents(string FilePath)
        {
            Helper.Log($"Emptying the {FilePath} file");
            System.IO.File.WriteAllText(FilePath, string.Empty);
        }
        public static bool TestFolderExist(string FolderPath)
        {
            Helper.Log($"Checking folder {FolderPath}");
            return System.IO.Directory.Exists(FolderPath);

        }
        

    }
}
