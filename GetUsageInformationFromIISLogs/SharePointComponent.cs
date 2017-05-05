using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace GetUsageInformationFromIISLogs
{
    class SharePointComponent
    {
        
        public static void TestCredentails()
        {
            if (Program.doSPCredentialCheck)
            {
                //foreach of the site in the list 
                foreach (string site in Program.SPSiteCollectionList)

                {
                    Console.ForegroundColor = ConsoleColor.Green;

                    Helper.Log(string.Format("Checking Permissions on site: {0}", site));
                    
                    //create client context
                    string Status = "";

                    try
                    {
                            ClientContext ctx = new ClientContext(site);
                            SharePointComponent.GetCreds(ref ctx);
                            ctx.Load(ctx.Web, w => w.Title);
                            ctx.ExecuteQuery();
                            Status = "Permission Check Passed !";
                            Helper.Log(Status);
                        
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Status = string.Format("Permission Check Failed ! error:{0}", e.Message);
                        Helper.Log(Status);

                    }


                }
            }

            Console.ResetColor();
            
        }
        private static void GetCreds(ref ClientContext SPCtx)
        {
            if (Program.isSPO)
            {
                //set the credentials property to Sharepoint online cred

                SPCtx.Credentials = new SharePointOnlineCredentials(Program.SPUsername, Program.SPPassword);

            }
            else {
                //create a network credential
                NetworkCredential cred = new NetworkCredential(Program.SPUsername.ToString(), Program.SPPasswordasString);
                SPCtx.Credentials = cred;
            }
        }
        public static void StartWork()
        {
            foreach (string site in Program.SPSiteCollectionList)
            {
                //split the string to get the url and iis id
                string[] str1 = site.Split(',');
                string url = str1[0];
                string IISid = str1[1];


                using (ClientContext ctx = new ClientContext(url))
                {

                    //assign appropirate creds
                    if (Program.isSPO)
                    {
                        //set the credentials property to Sharepoint online cred

                        ctx.Credentials = new SharePointOnlineCredentials(Program.SPUsername, Program.SPPassword);

                    }
                    else
                    {
                        //create a network credential
                        NetworkCredential cred = new NetworkCredential(Program.SPUsername.ToString(), Program.SPPasswordasString);
                        ctx.Credentials = cred;
                    }

                    //load the web, with only 2 props
                    ctx.Load(ctx.Web, wb => wb.ServerRelativeUrl, wb=>wb.Webs);
                    ctx.ExecuteQuery();
                    
                    // root web taken care
                    Helper.Log(string.Format("Root Site: {0}", ctx.Web.ServerRelativeUrl));
                    Helper.LogWeb(string.Format("{0}:{1} ", IISid, ctx.Web.ServerRelativeUrl));

                    //for each subsite , show urls
                    foreach (Web wb in ctx.Web.Webs)
                    {
                        GetWebs(wb,IISid,ctx);
                    }
                        
                }
                    
                
            }
        }

        public static void GetWebs(Web wb,string IISid, ClientContext ctx)
        {
            ctx.Load(wb, w => w.ServerRelativeUrl, w=>w.Webs);
            ctx.ExecuteQuery();
            Helper.Log(string.Format(wb.ServerRelativeUrl));
            Helper.LogWeb(string.Format("{0}:{1}", IISid, wb.ServerRelativeUrl));

            if (wb.Webs.Count != 0)
            {
                foreach (Web web1 in wb.Webs)
                {
                    GetWebs(web1,IISid,ctx);
                }

            }
        }
        
    }
}
