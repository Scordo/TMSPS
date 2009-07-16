using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace TMSPS.Core.Common
{
    public class BlackListReader
    {
        public static HashSet<string> GetBlackListedLogins(string xmlFileContent)
        {
            XmlReaderSettings objSettings = new XmlReaderSettings { IgnoreWhitespace = true };
            HashSet<string> result = new HashSet<string>();


            using (XmlReader objXmlReader = XmlReader.Create(new StringReader(xmlFileContent), objSettings))
            {
                objXmlReader.ReadStartElement("blacklist");

                while (objXmlReader.Name == "player")
                    result.Add(((XElement)XNode.ReadFrom(objXmlReader)).Element("login").Value);
            }

            return result;
        }

        public static HashSet<string> GetBlackListedLogins(Uri url)
        {
            string fileContent;
            WebClient webClient = new WebClient();

            try
            {
                fileContent = webClient.DownloadString(url);
            }
            catch (Exception ex)
            {
                throw new WebException("Error downloading the blacklist file from url: " + url, ex);
            }

            return GetBlackListedLogins(fileContent);
        }
    }
}
