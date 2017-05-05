using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetUsageInformationFromIISLogs
{
    class IISComponent
    {
        public static void StartWork()
        {
            //read the output file, foreach record, create a .sql file
            string[] allWebs = System.IO.File.ReadAllLines(Program.SPWeboutputFileWithPath);

            string IISSiteID = "";
            string webRelURL = "";
            //create all the sql files for each web
            foreach (string line in allWebs)
            {
                string[] temp = line.Split(':');
                IISSiteID = temp[0].Trim();
                webRelURL = temp[1].Trim();

                if (webRelURL.Trim() == "/".Trim())
                {
                    //skip this iteration because its default site coll '/'
                    continue;

                }
                else {
                    //for all the non root webs, create the .sql file
                    CreateSQLFileForWebURL(webRelURL, IISSiteID);
                }
                
            }

            // create master powershell ps1 file referencing the .sql file
            CreateMasterPowershellScript();
        }
        public static void CreateSQLFileForWebURL(string webRelativeURL, string IISid)
        {
            //this will create a .sql file with passed parameter as the "where" condition
            // Generi Query
            //  SELECT count(cs-uri-stem) AS HITS
            //  FROM {app.config key "IISLogsFolder"}
            //  WHERE cs-uri - stem like '/{replace with passed parameter}/%'
            //  now save the file as {webRelativeURL}.sql at folder specified in app.config key "PSSQLFileLocation"
            //
            Helper.Log($"Creating SQL file for -{webRelativeURL}--");
            string SELECT = "SELECT count(cs-uri-stem) AS HITS" +Environment.NewLine;
            string INTO = $"INTO {Program.LogParserOutFolder}\\{webRelativeURL.Replace('/','_')}.csv" + Environment.NewLine;
            string FROM = $"FROM {IISComponent.GetFolderPath(IISid)}" + Environment.NewLine;
            string WHERE = $"WHERE cs-uri-stem like '{webRelativeURL}/%'";

            string FinalSQLQuery = SELECT+INTO + FROM + WHERE;
            string SQLFileName = Program.PSSQLFileLocation +"\\" +webRelativeURL.Replace('/', '_') + ".sql";
             
            System.IO.File.WriteAllText(SQLFileName,FinalSQLQuery);

            Helper.Log($"Finished creating SQL file at {SQLFileName}");
        }
        private static string GetFolderPath(string IISid)
        {
            string folderpath = "";

            Helper.Log($"Finding IIS Log folder for IIS Site {IISid}");

            System.IO.DirectoryInfo DI = new System.IO.DirectoryInfo(Program.IISLogsFolder);

            var Alldirs = DI.EnumerateDirectories().Where(d=>d.Name.Contains(IISid)).First();

            folderpath = $"{Alldirs.FullName}\\*.log";

            Helper.Log($"IIS logs found at {folderpath}");

            return folderpath;
        }
        public static void CreateMasterPowershellScript()
        {
            Helper.Log($"Creating Master powershell");

            string head = "function getStat() {" +Environment.NewLine+
                                    @"Set-Location -Path 'C:\Program Files (x86)\Log Parser 2.2'" +Environment.NewLine ;

            System.IO.DirectoryInfo DI = new System.IO.DirectoryInfo(Program.PSSQLFileLocation);

            var AllSQLFiles = DI.EnumerateFiles();
            string body = "";

            foreach (var item in AllSQLFiles)
            {
                body = body + $".\\LogParser.exe -i:IISW3C file:{item.FullName} -o:csv" + Environment.NewLine;
                Helper.Log($"{body}");
            }

            string tail = "}" + Environment.NewLine + "getStat" +Environment.NewLine;
            string PSSCript = head + body + tail;

            System.IO.File.WriteAllText($"{Program.PSScriptLocation}\\master.ps1", PSSCript);
        }
        public static void RunningMasterPSToGenerateHits()
        {
            Helper.Log("Run the Master PS on each server to generate the Hits");


        }
    }
}
