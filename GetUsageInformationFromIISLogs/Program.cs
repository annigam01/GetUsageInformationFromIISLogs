using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace GetUsageInformationFromIISLogs
{
    class Program
    {
        public static string GlobalLogFilepath = ""; // has the path where the log files are kept
        public static bool isLoggingOn = true; // if true then logging is on
        public static string[] SPSiteCollectionList = null; // all the site collections that needs to queried for all webs
        public static string SPWeboutputFileWithPath = ""; // file with path where the detected webs will be stored
        public static string SPUsername = ""; // spusername
        public static SecureString SPPassword = null; // password
        public static string SPPasswordasString = null; // password

        public static bool doSPCredentialCheck = true; // should we check if the credentials are correct ? default yes
        public static bool isSPO = true ; // are we running for sharepoint online ?
        public static string PSSQLFileLocation = ""; // "IIS component" will create all the .sql file here
        public static string PSScriptLocation = ""; // "IIS component" will create the .ps1 file here
        public static string IISLogsFolder = ""; // "IIS component" will query the logs from here
        public static string LogParserOutFolder = "" ;// this is where the log parser will ouput the csv file with hits
        public static void Startup()
        {
            //this class will run all the one-time jobs at startup of application

            // 1. set the log file location at global 
            GlobalLogFilepath = Helper.SetFilePath();

            // 2. is logging ON ?
            isLoggingOn = Helper.SetLogging();

            // 3. user will give all the list of site collection under which all the webs has to be detected
            SPSiteCollectionList = System.IO.File.ReadAllLines(ConfigurationManager.AppSettings["SPSiteCollectionFileLocation"]);

            //4. after detecting all the webs, we will flush it to a text file, for IIS module to pickup
            SPWeboutputFileWithPath = ConfigurationManager.AppSettings["SPWeboutputFileWithPath"];
                //empty the file 
            Helper.ClearFileContents(SPWeboutputFileWithPath);

            // 5. sp username password
            SPUsername = ConfigurationManager.AppSettings["SPUsername"];
            SPPasswordasString = ConfigurationManager.AppSettings["SPPassword"];
            SPPassword = Helper.ConvertToSecureString(Program.SPPasswordasString);

            //6. should we check if the credentials are right?
            doSPCredentialCheck = bool.Parse(ConfigurationManager.AppSettings["doSPCredentialCheck"]);
            
            //run the sharepoint permissions test
            SharePointComponent.TestCredentails();

            // are we running for sharepoint online ?
            isSPO = bool.Parse(ConfigurationManager.AppSettings["isSPO"]);
            
            //whats the folder location where we want to save the .sql file?
            PSSQLFileLocation = ConfigurationManager.AppSettings["PSSQLFileLocation"];
            Helper.TestFolderExist(Program.PSScriptLocation);
            
            //where should the resulting .ps1 file be created?
            PSScriptLocation = ConfigurationManager.AppSettings["PSScriptLocation"];
            Helper.TestFolderExist(Program.PSScriptLocation);

            //where is the IIS logs folder, we assume that in this folder there are subfolders for each IIS website, each folders name has site's ID ( as in IIS)
            IISLogsFolder = ConfigurationManager.AppSettings["IISLogsFolder"];
            Helper.TestFolderExist(Program.IISLogsFolder);

            //where should the Log parser output its file ?
            LogParserOutFolder = ConfigurationManager.AppSettings["LogParserOutFolder"];
            Helper.TestFolderExist(Program.LogParserOutFolder);
        }
        static void Main(string[] args)
        {
            //initalise all variables for execution
            Startup();

            Helper.Log(".Starting application");

            Helper.Log("..Starting Step1- Querying SharePoint for All Webs/Nested Webs in SiteCollections");
            SharePointComponent.StartWork();
            
            Helper.Log("..Starting Step2- Quering IIS Logs for All the Webs");
            IISComponent.StartWork();

            Helper.Log(".Ending Application");
            Console.Read();


        }
    }
}
