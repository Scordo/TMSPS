using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using TMSPS.Core.Communication;

namespace TMSPS.Core.PluginSystem
{
    public class MessageConstants : Dictionary<string, string>
    {
        #region Constructor

        private MessageConstants()
        {
            
        }

        #endregion

        #region Public Methods

        public static MessageConstants ReadFromFile(string xmlConfigurationFile, TrackManiaRPCClient rpcClient)
        {
            if (rpcClient == null)
                throw new ArgumentNullException("rpcClient");

            MessageConstants result = new MessageConstants();
            result["servername"] = rpcClient.Methods.GetServerName().Value;

            if (!File.Exists(xmlConfigurationFile))
                return result;

            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            foreach (XNode node in configDocument.Root.Nodes())
            {
                if (node is XElement)
                    result[((XElement)node).Name.ToString().ToLower()] = ((XElement)node).Value;
            }

            return result;
        }

        #endregion
    }
}