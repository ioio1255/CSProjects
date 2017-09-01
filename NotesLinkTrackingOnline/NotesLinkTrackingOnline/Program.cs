using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NotesLinkTrackingOnline
{
    class Program
    {
        static readonly string SiteFileName = "SiteUrl.txt";
        static readonly string WebPartFileName = "WebPart.xml";
        static readonly string LinkFileName = "link.html";
        static readonly string PageTitle = "Notes Link Tracking.aspx";
        static readonly string LinkTrackingListName = "NotesLinkTracking";

        static void Main(string[] args)
        {
            Logs.InitialLogFile();
            Console.Write("Please input the login account name: ");
            string userName = Console.ReadLine();
            Console.Write("Please input the login account password: ");
            string password = GetPasswordFromConsole();
            Logs.LogToFile(LogLevel.Info, DateTime.Now, "Start...");
            Console.WriteLine("Start...");

            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SiteFileName);
            var siteUrlList = GetSitesUrl(filePath);
            siteUrlList.ForEach(siteUrl =>
            {
                Logs.LogMessage(siteUrl);
                Console.WriteLine("\nStart to deploy solution in {0}.", siteUrl);
                using (var context = new ClientContext(siteUrl))
                {
                    try
                    {
                        var securePWD = GetSecurePassword(password);
                        context.Credentials = new SharePointOnlineCredentials(userName, securePWD);
                        var list = GetList(context, LinkTrackingListName);
                        var page = GetOrCreateWebPartPage(context, list.Title, PageTitle);
                        var linkFile = GetorUploadLinkFile(context, list.Title, LinkFileName);
                        if (page.IsNewCreated)
                        {
                            AddWebPart(context, page.Path, linkFile.Path, WebPartFileName);
                            Logs.LogToFile(LogLevel.Info, DateTime.Now, string.Format("Succeed to deploy for the site collection: {0}. ", siteUrl));
                        }
                        else
                        {
                            Logs.LogToFile(LogLevel.Warning, DateTime.Now, "The page has existed and no need to be updated.");
                        }
                        Console.WriteLine("Successfully deployed the solution.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to deploy the solution in the site collection: {0}", siteUrl);
                        Logs.LogToFile(LogLevel.Error, DateTime.Now, string.Format("Failed to deploy for the site collection: {0}. Reason:{1} ", siteUrl, ex.ToString()));
                    }
                }
            });
            Console.WriteLine("Click any key to exit...");
            Console.ReadKey();
        }

        #region SharePoint Methods
        static void AddWebPart(ClientContext context, string pageUrl, string linkFileUrl, string webpartXmlFile = "WebPart.xml")
        {
            var zoneid = "Header";
            var zoneIndex = 1;

            Logs.LogToFile(LogLevel.Info, DateTime.Now, string.Format("Start to add Content Editor WebPart to the page: {0}. ", pageUrl));
            var page = context.Web.GetFileByServerRelativeUrl(pageUrl);
            var wpm = page.GetLimitedWebPartManager(PersonalizationScope.Shared);

            Logs.LogToFile(LogLevel.Info, DateTime.Now, "Start to update the WebPart template.");
            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, webpartXmlFile);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            var xmlNode = xmlDoc.GetElementsByTagName("ContentLink")[0];
            xmlNode.InnerText = linkFileUrl;
            var webPartSchemaXml = xmlDoc.OuterXml;
            Logs.LogToFile(LogLevel.Info, DateTime.Now, "The WebPart template is: " + webPartSchemaXml);

            var importedWebPart = wpm.ImportWebPart(webPartSchemaXml);
            var webPart = wpm.AddWebPart(importedWebPart.WebPart, zoneid, zoneIndex);
            context.ExecuteQuery();
        }

        static File GetSPOFile(ClientContext context, string listTitle, string filename = "link.html")
        {
            var web = context.Web;
            context.Load(web, x => x.Lists);
            context.ExecuteQuery();
            var pageLib = web.Lists.GetByTitle(listTitle);

            context.Load(pageLib, x => x.RootFolder);
            context.ExecuteQuery();
            var fileUrl = pageLib.RootFolder.ServerRelativeUrl + "/" + filename;
            File file = null;
            try
            {
                file = pageLib.RootFolder.Files.GetByUrl(fileUrl);
                context.Load(file);
                context.ExecuteQuery();
                Logs.LogToFile(LogLevel.Info, DateTime.Now, "The file has already existed. File url: " + fileUrl);
            }
            catch
            {
                Logs.LogToFile(LogLevel.Warning, DateTime.Now, "Cannot fild the file. File name: " + filename);
                file = null;

            }
            return file;
        }

        private static SPFileInfo GetorUploadLinkFile(ClientContext context, string listTitle, string filename = "link.html")
        {
            Logs.LogToFile(LogLevel.Info, DateTime.Now, "Start to get the WebPart link file information. File name: " + filename);
            var fileInfo = new SPFileInfo();
            var file = GetSPOFile(context, listTitle, filename);
            if (file == null)
            {
                Logs.LogToFile(LogLevel.Info, DateTime.Now, "Start to upload the WebPart link file. File name:  " + filename);
                var web = context.Web;
                context.Load(web, x => x.Lists);
                context.ExecuteQuery();
                var pageLib = web.Lists.GetByTitle(listTitle);
                context.Load(pageLib, x => x.RootFolder);
                context.ExecuteQuery();

                var fileUrl = pageLib.RootFolder.ServerRelativeUrl + "/" + filename;
                var fci = new FileCreationInformation();
                var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                fci.Content = System.IO.File.ReadAllBytes(filePath);
                fci.Url = fileUrl;
                file = pageLib.RootFolder.Files.Add(fci);
                context.Load(file);
                context.ExecuteQuery();
                Logs.LogToFile(LogLevel.Info, DateTime.Now, "The file has been uploaded. File name: " + filename);

                fileInfo.Path = file.ServerRelativeUrl;
                fileInfo.Title = file.Title;
                fileInfo.IsNewCreated = true;
            }
            else
            {
                fileInfo.Path = file.ServerRelativeUrl;
                fileInfo.Title = file.Title;
                fileInfo.IsNewCreated = false;
            }
            return fileInfo;
        }

        static SPFileInfo GetOrCreateWebPartPage(ClientContext context, string listTitle, string pageTitle)
        {
            Logs.LogToFile(LogLevel.Info, DateTime.Now, "Start to get the page information. Page name: " + pageTitle);
            var fileInfo = new SPFileInfo();
            var file = GetSPOFile(context, listTitle, pageTitle);
            if (file == null)
            {
                Logs.LogToFile(LogLevel.Info, DateTime.Now, "Start to create the page. Page name: " + pageTitle);
                var web = context.Web;
                context.Load(web, x => x.Lists);
                context.ExecuteQuery();
                var pageLib = web.Lists.GetByTitle(listTitle);
                context.Load(pageLib, x => x.RootFolder);
                context.ExecuteQuery();
                var fileUrl = pageLib.RootFolder.ServerRelativeUrl + "/" + pageTitle;
                file = pageLib.RootFolder.Files.AddTemplateFile(pageLib.RootFolder.ServerRelativeUrl + "/" + pageTitle, TemplateFileType.StandardPage);
                context.Load(file);
                context.ExecuteQuery();
                Logs.LogToFile(LogLevel.Info, DateTime.Now, "The page has been created. Page name: " + pageTitle);

                fileInfo.Title = file.Title;
                fileInfo.Path = file.ServerRelativeUrl;
                fileInfo.IsNewCreated = true;
            }
            else
            {
                fileInfo.Path = file.ServerRelativeUrl;
                fileInfo.Title = file.Title;
                fileInfo.IsNewCreated = false;
            }
            return fileInfo;
        }

        static List GetList(ClientContext context, string listTilte)
        {
            Logs.LogToFile(LogLevel.Info, DateTime.Now, "Start to get list information. List name: " + listTilte);
            var web = context.Web;
            List list = null;
            try
            {
                list = web.Lists.GetByTitle(listTilte);
                context.Load(list);
                context.ExecuteQuery();
                Logs.LogToFile(LogLevel.Info, DateTime.Now, "Succeed to get the list information.");
            }
            catch
            {
                Logs.LogToFile(LogLevel.Warning, DateTime.Now, "The list does not exist. Create it.");
                list = web.Lists.Add(
                   new ListCreationInformation()
                   {
                       Title = listTilte,
                       TemplateType = (int)ListTemplateType.DocumentLibrary
                   });
                context.Load(list);
                context.ExecuteQuery();
                Logs.LogToFile(LogLevel.Info, DateTime.Now, "The list has been created successfully.");
            }
            return list;
        }
        #endregion

        #region Common Methods
        static List<string> GetSitesUrl(string filePath)
        {
            Logs.LogToFile(LogLevel.Info, DateTime.Now, "Start to get site urls from file: " + filePath);
            var siteList = new List<string>();
            if (System.IO.File.Exists(filePath))
            {
                using (var reader = new System.IO.StreamReader(filePath))
                {
                    string siteUrl;
                    do
                    {
                        siteUrl = reader.ReadLine();
                        if (!string.IsNullOrEmpty(siteUrl) && siteUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                        {
                            siteList.Add(siteUrl);
                            Logs.LogToFile(LogLevel.Info, DateTime.Now, string.Format("The site url {0} was added into the list.", siteUrl));
                        }
                    } while (siteUrl != null);
                }
            }
            else
            {
                Logs.LogToFile(LogLevel.Warning, DateTime.Now, "The site url file does not exist.");
            }
            return siteList;
        }

        static SecureString GetSecurePassword(string password)
        {
            var securePWD = new SecureString();
            foreach (char c in password.ToCharArray())
                securePWD.AppendChar(c);
            return securePWD;
        }

        static string GetPasswordFromConsole()
        {
            var input = string.Empty;
            while (true)
            {
                var ck = Console.ReadKey(true);
                if (ck.Key != ConsoleKey.Enter)
                {
                    if (ck.Key != ConsoleKey.Backspace)
                    {
                        input += ck.KeyChar.ToString();
                        Console.Write("*");
                    }
                    else
                    {
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    Console.WriteLine();
                    break;
                }
            }
            return input;
        }
        #endregion
    }

    public struct SPFileInfo
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public bool IsNewCreated { get; set; }
    }
}
